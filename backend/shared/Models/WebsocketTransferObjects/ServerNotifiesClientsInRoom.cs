namespace core.Models.WebsocketTransferObjects;

public class ServerNotifiesClientsInRoom : BaseTransferObject
{
    public int roomId { get; set; } //RoomId is used to let the client socket connection know which room to display the message in
    public string? message { get; set; }
}