namespace core;

public class ServerNotifiesClientsInRoom : BaseTransferObject
{
    public int roomId { get; set; }
    public string message { get; set; }
    
}