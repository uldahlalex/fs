using api.SharedApiModels;
using core.Models.DbModels;

namespace api.ServerEvents;

public class ServerNotifiesClientsInRoomSomeoneHasLeftRoom : BaseTransferObject
{
    public EndUser user { get; set; }
}