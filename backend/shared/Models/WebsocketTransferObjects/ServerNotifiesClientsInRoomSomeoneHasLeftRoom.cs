using core.Models.DbModels;

namespace core.Models.WebsocketTransferObjects;

public class ServerNotifiesClientsInRoomSomeoneHasLeftRoom : ServerNotifiesClientsInRoom
{
    public EndUser user { get; set; }
}