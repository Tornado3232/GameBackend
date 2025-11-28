namespace GameBackend.API.DTO
{
    public record EventDto(int UserId, string EventType, string Meta, DateTime TsUtc);
}
