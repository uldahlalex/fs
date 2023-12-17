using api.SharedApiModels;

namespace api.ServerEvents;

//todo refactor to serversendsnotificationtotopiclisteners

/**
 * Cases where new class is derived:
 * If the client has different behavior based on the notification
 * (should it still be notification - or could there be a "just notify"?)
 * - prolly not, since the client should know how to treat the message
 */
//todo delete?
public abstract class ServerNotifiesClientsInRoom : BaseTransferObject
{
    public int
        roomId
    {
        get;
        set;
    } //RoomId is used to let the client socket connection know which room to display the message in

    public string? message { get; set; }
}