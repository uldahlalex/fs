using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using api.Abstractions;
using api.Attributes.EventFilters;
using api.Models;
using api.Models.ServerEvents;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Externalities;
using Externalities.ParameterModels;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToAuthenticateDto : BaseDto
{
    [EmailAddress] [Required] public string? email { get; set; }

    [MinLength(6)] [Required] public string? password { get; set; }
}

[RateLimit(3, 60)]
public class ClientWantsToAuthenticate(ChatRepository chatRepository) : BaseEventHandler<ClientWantsToAuthenticateDto>
{
    public override async Task Handle(ClientWantsToAuthenticateDto request, IWebSocketConnection socket)
    {
        var user = chatRepository.GetUser(new FindByEmailParams(request.email!));
        if (user.isbanned) throw new AuthenticationException("User is banned");
        var expectedHash = SecurityUtilities.Hash(request.password!, user.salt!);
        if (!expectedHash.Equals(user.hash)) throw new AuthenticationException("Wrong credentials!");
        socket.Authenticate(user);
        socket.SendDto(new ServerAuthenticatesUser { jwt = SecurityUtilities.IssueJwt(user) });
    }
}