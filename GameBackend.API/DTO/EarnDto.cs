using GameBackend.API.Models;

namespace GameBackend.API.DTO
{
    public record EarnDto(int UserId, int Amount, int EventId);
}
