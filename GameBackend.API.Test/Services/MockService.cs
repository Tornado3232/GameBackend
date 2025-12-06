using GameBackend.API.Abstractions;
using GameBackend.API.Models;
using Moq;

namespace GameBackend.API.Test.Services
{
    public static class MockService
    {
        public static Mock<IJwtService> MockJwt()
        {
            var mock = new Mock<IJwtService>();

            mock.Setup(x => x.CreatePasswordHash(It.IsAny<string>(),
                                                 out It.Ref<byte[]>.IsAny,
                                                 out It.Ref<byte[]>.IsAny))
                .Callback(new CreatePasswordHashCallback(
                    (string password, out byte[] hash, out byte[] salt) =>
                    {
                        hash = new byte[] { 1, 2, 3 };
                        salt = new byte[] { 4, 5, 6 };
                    }));

            mock.Setup(x => x.VerifyPasswordHash("correct", It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .Returns(true);

            mock.Setup(x => x.VerifyPasswordHash("wrong", It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .Returns(false);

            mock.Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("FAKE_JWT_TOKEN");

            return mock;
        }

        private delegate void CreatePasswordHashCallback(string password, out byte[] hash, out byte[] salt);
    }
}
