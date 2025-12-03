using GameBackend.API.Helpers;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace GameBackend.API.Services
{
    public class AppsFlyerService
    {
        private readonly AppsFlyerSettings _afSettings;

        public AppsFlyerService(IOptions<AppsFlyerSettings> afSettings)
        {
            _afSettings = afSettings.Value;
        }

        public bool VerifySignature(string rawBody, string receivedSignature)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_afSettings.Secret);
            var bodyBytes = Encoding.UTF8.GetBytes(rawBody);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(bodyBytes);

            string computedSignature = Convert.ToHexString(hashBytes).ToLower();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(receivedSignature),
                Encoding.UTF8.GetBytes(computedSignature)
            );
        }
    }
}
