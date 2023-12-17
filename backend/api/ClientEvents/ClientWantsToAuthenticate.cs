using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using api.ExtensionMethods;
using api.ServerEvents;
using api.SharedApiModels;
using Infrastructure;
using JetBrains.Annotations;
using MediatR;

namespace api.ClientEvents;

public class ClientWantsToAuthenticate : BaseTransferObject
{
    [EmailAddress] [Required] public string? email { get; set; }

    [MinLength(6)] [Required] public string? password { get; set; }
}

[UsedImplicitly] 
public class ClientWantsToAuthenticateHandler(ChatRepository chatRepository)
    : IRequestHandler<EventTypeRequest<ClientWantsToAuthenticate>>
{
    public Task Handle(EventTypeRequest<ClientWantsToAuthenticate> request, CancellationToken cancellationToken)
    {
        var user = chatRepository.GetUser(request.MessageObject.email!);
        var expectedHash = SecurityUtilities.SecurityUtilities.Hash(request.MessageObject.password!, user.salt!);
        if (!expectedHash.Equals(user.hash)) throw new AuthenticationException("Wrong credentials!");
        var jwt = SecurityUtilities.SecurityUtilities.IssueJwt(new Dictionary<string, object?>
            { { "email", user.email }, { "id", user.id } }!);
        request.Socket.Authenticate(user);
        request.Socket.SendDto(new ServerAuthenticatesUser { jwt = jwt });
        return Task.CompletedTask;
    }
}