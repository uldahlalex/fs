using core.ExtensionMethods;
using core.Models.WebsocketTransferObjects;
using core.SecurityUtilities;
using Infrastructure;
using JetBrains.Annotations;
using MediatR;

namespace api;

public class ClientWantsToAuthenticateWithJwt : BaseTransferObject
{
    public string? jwt { get; set; }
}

[UsedImplicitly]
public class ClientWantsToAuthenticateWithJwtHandler(ChatRepository chatRepository) : IRequestHandler<EventTypeRequest<ClientWantsToAuthenticateWithJwt>>
{
    public Task Handle(EventTypeRequest<ClientWantsToAuthenticateWithJwt> request, CancellationToken cancellationToken)
    {
        if (!SecurityUtilities.IsJwtValid(request.MessageObject.jwt!))
        {
            request.Socket.UnAuthenticate();
            return Task.CompletedTask;
        }

        var email = SecurityUtilities.ExtractClaims(request.MessageObject.jwt!)["email"];
        var user = chatRepository.GetUser(email);
        if (user.isbanned)
        {
            request.Socket.UnAuthenticate();
            return Task.CompletedTask;
        }

        request.Socket.Authenticate(user);
        request.Socket.SendDto(new ServerAuthenticatesUser { jwt = request.MessageObject.jwt });
        return Task.CompletedTask;
    }
}