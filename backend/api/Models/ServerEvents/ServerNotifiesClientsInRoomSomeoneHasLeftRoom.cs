using api.Models.DbModels;

namespace api.Models.ServerEvents;

public class ServerNotifiesClientsInRoomSomeoneHasLeftRoom : BaseTransferObject
{
    public EndUser user { get; set; }
}