namespace GameBackend.API.Models
{
    public class IdempotencyRecord
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string ResponseBody { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
