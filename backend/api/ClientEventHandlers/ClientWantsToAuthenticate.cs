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

public class ClientWantsToAuthenticateDto : BaseTransferObject
{
    [EmailAddress] [Required] public string? email { get; set; }

    [MinLength(6)] [Required] public string? password { get; set; }
}

public class ClientWantsToAuthenticate(ChatRepository chatRepository) :  BaseEventHandler<ClientWantsToAuthenticateDto>
{
    
    
    public override Task Handle(ClientWantsToAuthenticateDto request, IWebSocketConnection socket)
    {
        var user = chatRepository.GetUser(request.email!);
        var expectedHash = SecurityUtilities.Hash(request.password!, user.salt!);
        if (!expectedHash.Equals(user.hash)) throw new AuthenticationException("Wrong credentials!");
        var jwt = SecurityUtilities.IssueJwt(new Dictionary<string, object?>
            { { "email", user.email }, { "id", user.id } }!);
        socket.Authenticate(user);
        socket.SendDto(new ServerAuthenticatesUser { jwt = jwt });
        return Task.CompletedTask;
    }
    
    
}
