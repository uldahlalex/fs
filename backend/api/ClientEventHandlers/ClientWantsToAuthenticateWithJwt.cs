using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using api.Abstractions;
using api.Attributes.EventFilters;
using api.Externalities;
using api.Models;
using api.Models.ServerEvents;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToAuthenticateWithJwtDto : BaseDto
{
    [Required] public string? jwt { get; set; }
}

[RateLimit(6, 60)]
public class ClientWantsToAuthenticateWithJwt(ChatRepository chatRepository)
    : BaseEventHandler<ClientWantsToAuthenticateWithJwtDto>
{
    public override async Task Handle(ClientWantsToAuthenticateWithJwtDto dto, IWebSocketConnection socket)
    {
        var claims = SecurityUtilities.ValidateJwtAndReturnClaims(dto.jwt!);
        var user =  await chatRepository.GetUser(new FindByEmailParams(claims["email"]));
        if (user.isbanned)
            throw new AuthenticationException("User is banned");
        
        socket.Authenticate(user);
        socket.SendDto(new ServerAuthenticatesUser { jwt = dto.jwt });
    }
}