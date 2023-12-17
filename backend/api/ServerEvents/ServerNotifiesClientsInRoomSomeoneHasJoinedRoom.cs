using core.Models.DbModels;

namespace api.ServerEvents;

public class ServerNotifiesClientsInRoomSomeoneHasJoinedRoom : ServerNotifiesClientsInRoom
{
    public EndUser user { get; set; }
}