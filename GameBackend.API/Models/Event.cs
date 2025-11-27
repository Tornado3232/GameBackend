using System.ComponentModel.DataAnnotations.Schema;

namespace GameBackend.API.Models
{
    public class Event
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string EventType { get; set; } = null!;
        public DateTime TsUtc { get; set; }
        public string? Meta { get; set; }
    }
}
