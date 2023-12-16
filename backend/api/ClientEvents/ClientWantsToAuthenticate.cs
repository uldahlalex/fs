using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using core.ExtensionMethods;
using core.Models.DbModels;
using core.Models.WebsocketTransferObjects;
using core.SecurityUtilities;
using Fleck;
using Infrastructure;
using MediatR;
using Serilog;
using JetBrains.Annotations;

namespace api;

public class ClientWantsToAuthenticate : BaseTransferObject
{
    public IWebSocketConnection Socket { get; set; }
    [EmailAddress] [Required] public string? email { get; set; }

    [MinLength(6)] [Required] public string? password { get; set; }
}

[UsedImplicitly] //Invoked through MediatR
public class ClientWantsToAuthenticateHandler(ChatRepository chatRepository) : IRequestHandler<EventTypeRequest<ClientWantsToAuthenticate>>
{
    public Task Handle(EventTypeRequest<ClientWantsToAuthenticate> request, CancellationToken cancellationToken)
    {
        EndUser user;
        try
        {
            user =  chatRepository.GetUser(request.MessageObject.email!);
        }
        catch (Exception e)
        {
            Log.Error(e, "WebsocketServer");
            throw new AuthenticationException("User does not exist!");
        }

        var expectedHash = SecurityUtilities.Hash(request.MessageObject.password, user.salt!);
        if (!expectedHash.Equals(user.hash)) throw new AuthenticationException("Wrong password!");
        var jwt = SecurityUtilities.IssueJwt(new Dictionary<string, object?>
            { { "email", user.email }, { "id", user.id } });
        request.Socket.Authenticate(user);
        request.Socket.SendDto(new ServerAuthenticatesUser { jwt = jwt });
        return Task.CompletedTask;
    }
}

