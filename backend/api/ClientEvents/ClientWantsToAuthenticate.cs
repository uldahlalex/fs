using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using api.ServerEvents;
using api.SharedApiModels;
using core;
using core.ExtensionMethods;
using core.Models.DbModels;
using core.SecurityUtilities;
using Infrastructure;
using JetBrains.Annotations;
using MediatR;
using Serilog;

namespace api.ClientEvents;

public class ClientWantsToAuthenticate : BaseTransferObject
{
    [EmailAddress] [Required] public string? email { get; set; }

    [MinLength(6)] [Required] public string? password { get; set; }
}

[UsedImplicitly] //Invoked through MediatR
public class ClientWantsToAuthenticateHandler(ChatRepository chatRepository)
    : IRequestHandler<EventTypeRequest<ClientWantsToAuthenticate>>
{
    public Task Handle(EventTypeRequest<ClientWantsToAuthenticate> request, CancellationToken cancellationToken)
    {
        EndUser user = chatRepository.GetUser(request.MessageObject.email!);
        var expectedHash = SecurityUtilities.Hash(request.MessageObject.password!, user.salt!);
        if (!expectedHash.Equals(user.hash)) throw new AuthenticationException("Wrong password!");
        var jwt = SecurityUtilities.IssueJwt(new Dictionary<string, object?>
            { { "email", user.email }, { "id", user.id } });
        request.Socket.Authenticate(user);
        request.Socket.SendDto(new ServerAuthenticatesUser { jwt = jwt });
        return Task.CompletedTask;
    }
}