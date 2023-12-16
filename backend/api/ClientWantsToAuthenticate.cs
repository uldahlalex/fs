using System.Security.Authentication;
using core.ExtensionMethods;
using core.Models.DbModels;
using core.Models.WebsocketTransferObjects;
using core.SecurityUtilities;
using Fleck;
using Infrastructure;
using MediatR;
using Serilog;
using core.ExtensionMethods;

namespace api;


//todo hav type her i stedet for shared?

public class EventTypeRequest<T> : IRequest
{
    public WebSocketConnection Socket { get; set; }
    public T Dto { get; set; }
}
public class ClientWantsToAuthenticate(ChatRepository chatRepository) : IRequestHandler<EventTypeRequest<core.Models.WebsocketTransferObjects.ClientWantsToAuthenticate>>
{
    
    public Task Handle(EventTypeRequest<core.Models.WebsocketTransferObjects.ClientWantsToAuthenticate> request, CancellationToken cancellationToken)
    {

        EndUser user;
        try
        {
            user = chatRepository.GetUser(request.Dto.email!);
        }
        catch (Exception e)
        {
            Log.Error(e, "WebsocketServer");
            throw new AuthenticationException("User does not exist!");
        }

        var expectedHash = SecurityUtilities.Hash(request.Dto.password, user.salt!);
        if (!expectedHash.Equals(user.hash)) throw new AuthenticationException("Wrong password!");
        var jwt = SecurityUtilities.IssueJwt(new Dictionary<string, object?>
            { { "email", user.email }, { "id", user.id } });
        request.Socket.Authenticate(user);
        request.Socket.SendDto(new ServerAuthenticatesUser { jwt = jwt });
        return Task.CompletedTask;
    }
}