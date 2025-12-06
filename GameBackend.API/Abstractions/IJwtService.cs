using GameBackend.API.Models;

namespace GameBackend.API.Abstractions
{
    public interface IJwtService
    {
        void CreatePasswordHash(string password, out byte[] hash, out byte[] salt);
        bool VerifyPasswordHash(string password, byte[] hash, byte[] salt);
        string GenerateToken(User user);
    }
}
