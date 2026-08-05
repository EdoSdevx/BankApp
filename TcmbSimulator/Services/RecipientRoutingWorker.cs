using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TcmbSimulator.Configuration;
using TcmbSimulator.Data;

namespace TcmbSimulator.Services;

public class RecipientRoutingWorker : BackgroundService
{
    private const int BatchSize = 10;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RecipientRoutingOptions _options;
    private readonly ILogger<RecipientRoutingWorker> _logger;
    private readonly HashSet<string> _missingSecretWarnings = new();

    public RecipientRoutingWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RecipientRoutingOptions> options,
        ILogger<RecipientRoutingWorker> logger)
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
                _logger.LogError(exception, "Recipient routing polling failed.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds)),
                stoppingToken);
        }
    }

    private async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dataAccess = scope.ServiceProvider.GetRequiredService<IRoutingOutboxDataAccess>();
        var recipientClient = scope.ServiceProvider.GetRequiredService<IRecipientBankClient>();
        var messages = await dataAccess.GetPendingAsync(BatchSize, cancellationToken);

        foreach (var message in messages)
        {
            if (!_options.SharedSecrets.TryGetValue(
                    message.ReceiverBankCode,
                    out var sharedSecret) ||
                string.IsNullOrWhiteSpace(sharedSecret))
            {
                if (_missingSecretWarnings.Add(message.ReceiverBankCode))
                {
                    _logger.LogWarning(
                        "Routing to bank {ReceiverBankCode} is paused because its shared secret is missing.",
                        message.ReceiverBankCode);
                }

                continue;
            }

            _missingSecretWarnings.Remove(message.ReceiverBankCode);

            try
            {
                await dataAccess.MarkRoutingAsync(
                    message.RoutingOutboxMessageId,
                    cancellationToken);

                var response = await recipientClient.RouteAsync(
                    message.ToRequest(),
                    message.ReceiverApiBaseUrl,
                    sharedSecret,
                    cancellationToken);

                await dataAccess.MarkResultAsync(
                    message.RoutingOutboxMessageId,
                    response,
                    cancellationToken);

                _logger.LogInformation(
                    "Payment {CentralReference} was {Status} by bank {ReceiverBankCode}.",
                    message.CentralReference,
                    response.Status,
                    message.ReceiverBankCode);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                await dataAccess.MarkFailedAsync(
                    message.RoutingOutboxMessageId,
                    exception.Message,
                    Math.Max(1, _options.MaxAttempts),
                    cancellationToken);

                _logger.LogWarning(
                    exception,
                    "Routing payment {CentralReference} failed on attempt {AttemptCount}.",
                    message.CentralReference,
                    message.AttemptCount + 1);
            }
        }
    }
}
