using api;
using core.Models.DbModels;


namespace core.Models.WebsocketTransferObjects;

public class ServerNotifiesClientsInRoomSomeoneHasLeftRoom : BaseTransferObject
{
    public EndUser user { get; set; }
}