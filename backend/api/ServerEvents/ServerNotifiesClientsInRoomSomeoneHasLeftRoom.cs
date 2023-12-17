using api.SharedApiModels;
using Infrastructure.DbModels;

namespace api.ServerEvents;

public class ServerNotifiesClientsInRoomSomeoneHasLeftRoom : BaseTransferObject
{
    public EndUser user { get; set; }
}