using api.ClientEventHandlers;

namespace api.StaticHelpers;

public static class StaticValues
{

  
    
    public static ClientWantsToAuthenticateDto AuthEvent = new()
    {
        email = "bla@bla.dk",
        password = "qweqweqwe"
    };

    public static ClientWantsToEnterRoomDto EnterRoomEvent = new()
    {
        roomId = 1
    };

    public static ClientWantsToSendMessageToRoomDto SendMessageEvent = new()
    {
        roomId = 1,
        messageContent = "hey"
    };
}