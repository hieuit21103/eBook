namespace Shared;

public class UserStatusChangedEvent
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
}