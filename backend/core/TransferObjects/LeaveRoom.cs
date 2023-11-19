namespace core;

public class ServerLetsClientLeaveRoom : BaseTransferObject
{
    public int roomId { get; set; }
}

public class ClientWantsToLeaveRoom : BaseTransferObject
{
    public int roomId { get; set; }
}