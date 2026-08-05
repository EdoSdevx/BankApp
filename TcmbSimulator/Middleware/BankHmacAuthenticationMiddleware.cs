using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TcmbSimulator.Configuration;

namespace TcmbSimulator.Middleware;

public class BankHmacAuthenticationMiddleware
{
    public const string AuthenticatedBankCodeItem = "AuthenticatedBankCode";

    private readonly RequestDelegate _next;

    public BankHmacAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IOptions<BankAuthenticationOptions> options)
    {
        if (!context.Request.Path.StartsWithSegments("/api/payments"))
        {
            await _next(context);
            return;
        }

        if (!TryReadHeaders(context.Request, out var bankCode, out var timestamp, out var signature))
        {
            await WriteUnauthorizedAsync(context, "Missing or invalid bank authentication headers.");
            return;
        }

        var settings = options.Value;
        DateTimeOffset requestTime;
        try
        {
            requestTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        }
        catch (ArgumentOutOfRangeException)
        {
            await WriteUnauthorizedAsync(context, "The request timestamp is invalid.");
            return;
        }

        var age = (DateTimeOffset.UtcNow - requestTime).Duration();

        if (age > TimeSpan.FromSeconds(settings.AllowedClockSkewSeconds))
        {
            await WriteUnauthorizedAsync(context, "The request timestamp has expired.");
            return;
        }

        if (!settings.SharedSecrets.TryGetValue(bankCode, out var secret) ||
            string.IsNullOrWhiteSpace(secret))
        {
            await WriteUnauthorizedAsync(context, "The sender bank is not configured.");
            return;
        }

        context.Request.EnableBuffering();
        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var body = await reader.ReadToEndAsync(context.RequestAborted);
        context.Request.Body.Position = 0;

        var timestampText = timestamp.ToString(CultureInfo.InvariantCulture);
        var canonicalRequest = $"{bankCode}\n{timestampText}\n{body}";
        var expectedSignature = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(canonicalRequest));

        byte[] suppliedSignature;
        try
        {
            suppliedSignature = Convert.FromHexString(signature);
        }
        catch (FormatException)
        {
            await WriteUnauthorizedAsync(context, "The request signature is invalid.");
            return;
        }

        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, suppliedSignature))
        {
            await WriteUnauthorizedAsync(context, "The request signature is invalid.");
            return;
        }

        context.Items[AuthenticatedBankCodeItem] = bankCode;
        await _next(context);
    }

    private static bool TryReadHeaders(
        HttpRequest request,
        out string bankCode,
        out long timestamp,
        out string signature)
    {
        bankCode = request.Headers["X-Bank-Code"].ToString();
        signature = request.Headers["X-Signature"].ToString();

        timestamp = 0;
        return !string.IsNullOrWhiteSpace(bankCode) &&
               !string.IsNullOrWhiteSpace(signature) &&
               long.TryParse(
                   request.Headers["X-Timestamp"].ToString(),
                   NumberStyles.None,
                   CultureInfo.InvariantCulture,
                   out timestamp);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { message });
    }
}
