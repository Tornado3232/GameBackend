using GameBackend.API.Abstractions;
using GameBackend.API.Models;

namespace GameBackend.API.Test.Services
{
    internal class FakeJwtService : IJwtService
    {
        public void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            hash = new byte[] { 10, 20, 30 };
            salt = new byte[] { 40, 50, 60 };
        }
        public bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
        {
            return password == "correct";
        }

        public string GenerateToken(User user)
        {
            return "FAKE_JWT_TOKEN";
        }
    }
}
