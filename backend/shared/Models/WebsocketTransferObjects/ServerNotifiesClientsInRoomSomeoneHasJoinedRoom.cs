using core.Models.DbModels;

namespace core.Models.WebsocketTransferObjects;

public class ServerNotifiesClientsInRoomSomeoneHasJoinedRoom : ServerNotifiesClientsInRoom
{
    public EndUser user { get; set; }
}