using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GameBackend.API.Helpers
{
    public static class JwtGenerator
    {
        public static string Generate(string username, string secretKey, int expiresMinutes = 60)
        {
            var header = new { alg = "HS256", typ = "JWT" };
            var payload = new
            {
                sub = username,
                exp = DateTimeOffset.UtcNow.AddMinutes(expiresMinutes).ToUnixTimeSeconds()
            };

            string headerBase64 = Base64UrlEncode(JsonSerializer.Serialize(header));
            string payloadBase64 = Base64UrlEncode(JsonSerializer.Serialize(payload));

            string unsignedToken = $"{headerBase64}.{payloadBase64}";
            var signature = HmacSha256(unsignedToken, secretKey);

            return $"{unsignedToken}.{signature}";
        }

        private static string HmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Base64UrlEncode(hash);
        }

        private static string Base64UrlEncode(string input) =>
            Base64UrlEncode(Encoding.UTF8.GetBytes(input));

        private static string Base64UrlEncode(byte[] input) =>
            Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
    }
}
