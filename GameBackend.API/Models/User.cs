namespace GameBackend.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public int Balance { get; set; } = 0;
    }
}
