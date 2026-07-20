using System.Data;
using System.Text.Json;
using BankApp.BankApp.Common.Dtos.ExchangeRates;
using BankApp.BankApp.DataAccess;
using BankApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.BankApp.Services;

public class ExchangeRateUpdaterService : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExchangeRateUpdaterService> _logger;

    public ExchangeRateUpdaterService(
        IHubContext<NotificationHub> hubContext,
        IServiceScopeFactory scopeFactory,
        ILogger<ExchangeRateUpdaterService> logger)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(
                    "https://api.frankfurter.app/latest?from=TRY", stoppingToken);

                using var doc = JsonDocument.Parse(response);
                var rates = doc.RootElement.GetProperty("rates");

                var updatedRates = new List<ExchangeRateListDto>();

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                using var connection = dbContext.CreateConnection();
                await connection.OpenAsync(stoppingToken);

                var supportedCodes = new HashSet<string>();
                using (var cmd = new SqlCommand("SELECT CurrencyCode FROM Currencies", connection))
                using (var reader = await cmd.ExecuteReaderAsync(stoppingToken))
                {
                    while (await reader.ReadAsync(stoppingToken))
                        supportedCodes.Add(reader.GetString(0));
                }

                foreach (var rateProp in rates.EnumerateObject())
                {
                    var currencyCode = rateProp.Name;
                    if (!supportedCodes.Contains(currencyCode)) continue;
                    var rate = 1 / rateProp.Value.GetDecimal();

                    using var cmd = new SqlCommand(@"
                        MERGE ExchangeRates AS t
                        USING (SELECT @CurrencyCode AS CurrencyCode) AS s
                        ON t.CurrencyCode = s.CurrencyCode
                        WHEN MATCHED THEN UPDATE SET Rate = @Rate, RateDate = @RateDate, Source = @Source
                        WHEN NOT MATCHED THEN INSERT (CurrencyCode, Rate, RateDate, Source)
                        VALUES (@CurrencyCode, @Rate, @RateDate, @Source);", connection);

                    cmd.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = currencyCode;
                    cmd.Parameters.Add("@Rate", SqlDbType.Decimal).Value = rate;
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                    cmd.Parameters.Add("@Source", SqlDbType.NVarChar, 255).Value = "Frankfurter";

                    await cmd.ExecuteNonQueryAsync(stoppingToken);

                    updatedRates.Add(new ExchangeRateListDto
                    {
                        CurrencyCode = currencyCode,
                        Rate = rate,
                        RateDate = DateTime.UtcNow,
                        Source = "Frankfurter"
                    });
                }

                if (updatedRates.Count > 0)
                {
                    await _hubContext.Clients.All.SendAsync("RatesUpdated", updatedRates, stoppingToken);
                    _logger.LogInformation("Exchange rates updated: {Count} currencies from Frankfurter", updatedRates.Count);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to fetch exchange rates");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
