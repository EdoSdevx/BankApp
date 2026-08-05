using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TcmbSimulator.Configuration;
using TcmbSimulator.Contracts.Routing;

namespace TcmbSimulator.Services;

public class RecipientBankClient : IRecipientBankClient
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly RecipientRoutingOptions _options;

    public RecipientBankClient(
        HttpClient httpClient,
        IOptions<RecipientRoutingOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<RoutePaymentResponse> RouteAsync(
        RoutePaymentRequest request,
        string receiverApiBaseUrl,
        string sharedSecret,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(receiverApiBaseUrl, UriKind.Absolute, out var baseUri))
            throw new InvalidOperationException("Recipient bank API URL is invalid.");

        if (string.IsNullOrWhiteSpace(_options.SwitchCode))
            throw new InvalidOperationException("RecipientRouting:SwitchCode is missing.");

        if (string.IsNullOrWhiteSpace(sharedSecret))
            throw new InvalidOperationException("Recipient bank shared secret is missing.");

        var jsonBody = JsonSerializer.Serialize(request, JsonOptions);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestampText = timestamp.ToString(CultureInfo.InvariantCulture);
        var canonicalRequest = $"{_options.SwitchCode}\n{timestampText}\n{jsonBody}";
        var signature = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(sharedSecret),
            Encoding.UTF8.GetBytes(canonicalRequest)));

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(baseUri, "api/incoming-payments"));
        httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        httpRequest.Headers.Add("X-Switch-Code", _options.SwitchCode);
        httpRequest.Headers.Add("X-Timestamp", timestampText);
        httpRequest.Headers.Add("X-Signature", signature);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Recipient bank returned {(int)response.StatusCode} {response.StatusCode}: {responseBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<RoutePaymentResponse>(
                         JsonOptions,
                         cancellationToken)
                     ?? throw new InvalidOperationException(
                         "Recipient bank returned an empty response.");

        if (!string.Equals(
                result.CentralReference,
                request.CentralReference,
                StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Recipient bank returned a different central reference.");

        if (result.Status != "Completed" && result.Status != "Rejected")
            throw new InvalidOperationException(
                $"Recipient bank returned the unsupported status '{result.Status}'.");

        return result;
    }
}
