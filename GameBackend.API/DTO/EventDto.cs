namespace GameBackend.API.DTO
{
    public record EventDto(string EventType, string Meta, DateTime TsUtc);
}
