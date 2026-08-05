using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BankApp.BankApp.Common.Dtos.Eft.Switch;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.Common.Options;
using Microsoft.Extensions.Options;

namespace BankApp.BankApp.Services;

public class EftSwitchClient : IEftSwitchClient
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly EftSwitchOptions _options;

    public EftSwitchClient(
        HttpClient httpClient,
        IOptions<EftSwitchOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<SubmitEftResponseDto> SubmitAsync(
        SubmitEftRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var jsonBody = JsonSerializer.Serialize(request, JsonOptions);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestampText = timestamp.ToString(CultureInfo.InvariantCulture);
        var canonicalRequest = $"{_options.BankCode}\n{timestampText}\n{jsonBody}";
        var signature = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(_options.SharedSecret),
            Encoding.UTF8.GetBytes(canonicalRequest)));

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(new Uri(_options.BaseUrl), "api/payments"));
        httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        httpRequest.Headers.Add("X-Bank-Code", _options.BankCode);
        httpRequest.Headers.Add("X-Timestamp", timestampText);
        httpRequest.Headers.Add("X-Signature", signature);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"TCMB returned {(int)response.StatusCode} {response.StatusCode}: {responseBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<SubmitEftResponseDto>(
                         JsonOptions,
                         cancellationToken)
                     ?? throw new InvalidOperationException("TCMB returned an empty response.");

        if (!string.Equals(result.Status, "Accepted", StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"TCMB returned the unexpected payment status '{result.Status}'.");

        return result;
    }

    private void ValidateConfiguration()
    {
        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException("EftSwitch:BaseUrl must be an absolute URL.");

        if (_options.BankCode.Length != 5 || !_options.BankCode.All(char.IsDigit))
            throw new InvalidOperationException("EftSwitch:BankCode must contain five digits.");

        if (string.IsNullOrWhiteSpace(_options.SharedSecret))
            throw new InvalidOperationException("EftSwitch:SharedSecret is missing.");
    }
}
