namespace core.Models.WebsocketTransferObjects;


public class ClientWantsToLeaveRoom : BaseTransferObject
{
    public int roomId { get; set; }
}