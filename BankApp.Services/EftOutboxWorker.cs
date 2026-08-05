using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.Common.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BankApp.BankApp.Services;

public class EftOutboxWorker : BackgroundService
{
    private const int BatchSize = 10;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EftSwitchOptions _options;
    private readonly ILogger<EftOutboxWorker> _logger;
    private bool _configurationWarningLogged;

    public EftOutboxWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<EftSwitchOptions> options,
        ILogger<EftOutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "EFT outbox polling failed.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds)),
                stoppingToken);
        }
    }

    private async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        if (!HasUsableConfiguration())
        {
            if (!_configurationWarningLogged)
            {
                _logger.LogWarning(
                    "EFT outbox processing is paused because EftSwitch configuration is incomplete.");
                _configurationWarningLogged = true;
            }

            return;
        }

        _configurationWarningLogged = false;

        using var scope = _scopeFactory.CreateScope();
        var dataAccess = scope.ServiceProvider.GetRequiredService<IEftOutboxDataAccess>();
        var switchClient = scope.ServiceProvider.GetRequiredService<IEftSwitchClient>();
        var messages = await dataAccess.GetPendingAsync(BatchSize, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var response = await switchClient.SubmitAsync(
                    message.ToRequest(),
                    cancellationToken);

                await dataAccess.MarkSubmittedAsync(
                    message.OutboxMessageId,
                    response.CentralReference,
                    cancellationToken);

                _logger.LogInformation(
                    "EFT {EftTransferId} was accepted by TCMB as {CentralReference}.",
                    message.EftTransferId,
                    response.CentralReference);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                await dataAccess.MarkFailedAsync(
                    message.OutboxMessageId,
                    exception.Message,
                    Math.Max(1, _options.MaxAttempts),
                    cancellationToken);

                _logger.LogWarning(
                    exception,
                    "EFT {EftTransferId} submission failed on attempt {AttemptCount}.",
                    message.EftTransferId,
                    message.AttemptCount + 1);
            }
        }
    }

    private bool HasUsableConfiguration()
    {
        return Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out _) &&
               _options.BankCode.Length == 5 &&
               _options.BankCode.All(char.IsDigit) &&
               !string.IsNullOrWhiteSpace(_options.SharedSecret);
    }
}
