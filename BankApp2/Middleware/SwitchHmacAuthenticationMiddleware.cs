using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using BankApp2.Configuration;
using Microsoft.Extensions.Options;

namespace BankApp2.Middleware;

public class SwitchHmacAuthenticationMiddleware
{
    public const string AuthenticatedSwitchItem = "AuthenticatedSwitch";

    private readonly RequestDelegate _next;

    public SwitchHmacAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IOptions<SwitchAuthenticationOptions> options)
    {
        if (!context.Request.Path.StartsWithSegments("/api/incoming-payments"))
        {
            await _next(context);
            return;
        }

        var switchCode = context.Request.Headers["X-Switch-Code"].ToString();
        var timestampHeader = context.Request.Headers["X-Timestamp"].ToString();
        var suppliedSignatureText = context.Request.Headers["X-Signature"].ToString();

        if (string.IsNullOrWhiteSpace(switchCode) ||
            string.IsNullOrWhiteSpace(suppliedSignatureText) ||
            !long.TryParse(
                timestampHeader,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var timestamp))
        {
            await WriteUnauthorizedAsync(
                context,
                "Missing or invalid switch authentication headers.");
            return;
        }

        var settings = options.Value;
        if (!string.Equals(switchCode, settings.SwitchCode, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(settings.SharedSecret))
        {
            await WriteUnauthorizedAsync(context, "The payment switch is not configured.");
            return;
        }

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

        if ((DateTimeOffset.UtcNow - requestTime).Duration() >
            TimeSpan.FromSeconds(settings.AllowedClockSkewSeconds))
        {
            await WriteUnauthorizedAsync(context, "The request timestamp has expired.");
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
        var canonicalRequest = $"{switchCode}\n{timestampText}\n{body}";
        var expectedSignature = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(settings.SharedSecret),
            Encoding.UTF8.GetBytes(canonicalRequest));

        byte[] suppliedSignature;
        try
        {
            suppliedSignature = Convert.FromHexString(suppliedSignatureText);
        }
        catch (FormatException)
        {
            await WriteUnauthorizedAsync(context, "The request signature is invalid.");
            return;
        }

        if (!CryptographicOperations.FixedTimeEquals(
                expectedSignature,
                suppliedSignature))
        {
            await WriteUnauthorizedAsync(context, "The request signature is invalid.");
            return;
        }

        context.Items[AuthenticatedSwitchItem] = switchCode;
        await _next(context);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { message });
    }
}
