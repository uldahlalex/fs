using api.Models.DbModels;

namespace api.Models.ServerEvents;

public class ServerNotifiesClientsInRoomSomeoneHasJoinedRoom : ServerNotifiesClientsInRoom
{
    public EndUser user { get; set; }

}