using System.Security.Cryptography;
using System.Text;

namespace BankApp.BankApp.Common.Helpers;

public static class ResetTokenHelper
{
    private static readonly char[] Separator = ['|'];

    public static string GenerateToken(string entityType, int entityId, DateTime expiresAt, string secretKey)
    {
        var payload = $"{entityType}|{entityId}|{expiresAt.Ticks}";
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var secretBytes = Encoding.UTF8.GetBytes(secretKey);

        var hmacBytes = HMACSHA256.HashData(secretBytes, payloadBytes);

        var signatureUrl = Base64ToUrl(Convert.ToBase64String(hmacBytes));
        var payloadUrl = Base64ToUrl(Convert.ToBase64String(payloadBytes));

        return $"{payloadUrl}.{signatureUrl}";
    }

    public static (string EntityType, int EntityId)? ValidateToken(string token, string secretKey)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 2)
                return null;

            var payloadBytes = Convert.FromBase64String(UrlToBase64(parts[0]));
            var expectedSignature = Convert.FromBase64String(UrlToBase64(parts[1]));

            var secretBytes = Encoding.UTF8.GetBytes(secretKey);
            var computedSignature = HMACSHA256.HashData(secretBytes, payloadBytes);

            if (!CryptographicOperations.FixedTimeEquals(expectedSignature, computedSignature))
                return null;

            var payload = Encoding.UTF8.GetString(payloadBytes);
            var segments = payload.Split(Separator, 3);
            if (segments.Length != 3)
                return null;

            var entityType = segments[0];
            if (!int.TryParse(segments[1], out var entityId))
                return null;
            if (!long.TryParse(segments[2], out var expiryTicks))
                return null;

            var expiresAt = new DateTime(expiryTicks, DateTimeKind.Utc);
            if (DateTime.UtcNow > expiresAt)
                return null;

            return (entityType, entityId);
        }
        catch
        {
            return null;
        }
    }

    private static string Base64ToUrl(string base64)
    {
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static string UrlToBase64(string url)
    {
        var padded = url.Replace('-', '+').Replace('_', '/');
        var mod = padded.Length % 4;
        if (mod != 0)
            padded += new string('=', 4 - mod);

        return padded;
    }
}
