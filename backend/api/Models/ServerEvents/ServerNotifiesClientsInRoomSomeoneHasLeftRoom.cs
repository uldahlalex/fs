using api.Models.DbModels;

namespace api.Models.ServerEvents;

public class ServerNotifiesClientsInRoomSomeoneHasLeftRoom : BaseDto
{
    public EndUser user { get; set; }

    public int roomId { get; set; }

    public string? message { get; set; }
}