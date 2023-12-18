using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using api.ExtensionMethods;
using api.Helpers;
using api.Models;
using api.Models.ServerEvents;
using Infrastructure;
using JetBrains.Annotations;
using MediatR;

namespace api.ClientEventHandlers;

public class ClientWantsToAuthenticateWithJwt : BaseTransferObject
{
    [Required] public string? jwt { get; set; }
}

[UsedImplicitly]
public class ClientWantsToAuthenticateWithJwtHandler(ChatRepository chatRepository)
    : IRequestHandler<EventTypeRequest<ClientWantsToAuthenticateWithJwt>>
{
    public Task Handle(EventTypeRequest<ClientWantsToAuthenticateWithJwt> request, CancellationToken cancellationToken)
    {
        var claims = SecurityUtilities.ValidateJwtAndReturnClaims(request.MessageObject.jwt!);
        var user = chatRepository.GetUser(claims["email"]);
        if (user.isbanned)
            throw new AuthenticationException("User is banned");

        request.Socket.Authenticate(user);
        request.Socket.SendDto(new ServerAuthenticatesUser { jwt = request.MessageObject.jwt });
        return Task.CompletedTask;
    }
}