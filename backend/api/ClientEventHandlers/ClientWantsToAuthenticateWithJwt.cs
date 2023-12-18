using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using api.Abstractions;
using api.ExtensionMethods;
using api.Helpers;
using api.Models;
using api.Models.ServerEvents;
using Fleck;
using Infrastructure;

namespace api.ClientEventHandlers;

public class ClientWantsToAuthenticateWithJwtDto : BaseTransferObject
{
    [Required] public string? jwt { get; set; }
}

public class ClientWantsToAuthenticateWithJwt(ChatRepository chatRepository) : BaseEventHandler<ClientWantsToAuthenticateWithJwtDto>
{
    public override Task Handle(ClientWantsToAuthenticateWithJwtDto dto, IWebSocketConnection socket)
    {
        var claims = SecurityUtilities.ValidateJwtAndReturnClaims(dto.jwt!);
        var user = chatRepository.GetUser(claims["email"]);
        if (user.isbanned)
            throw new AuthenticationException("User is banned");

        socket.Authenticate(user);
        socket.SendDto(new ServerAuthenticatesUser { jwt = dto.jwt });
        return Task.CompletedTask;
    }
}