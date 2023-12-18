using System.ComponentModel.DataAnnotations;
using api.ExtensionMethods;
using api.Helpers;
using api.Models;
using api.Models.ServerEvents;
using Infrastructure;
using JetBrains.Annotations;
using MediatR;

namespace api.ClientEventHandlers;

public class ClientWantsToRegister : BaseTransferObject
{
    [EmailAddress] public string? email { get; set; }

    [MinLength(6)] public string? password { get; set; }
}

[UsedImplicitly]
public class ClientWantsToRegisterHandler(ChatRepository chatRepository)
    : IRequestHandler<EventTypeRequest<ClientWantsToRegister>>
{
    public Task Handle(EventTypeRequest<ClientWantsToRegister> request, CancellationToken cancellationToken)
    {
        if (chatRepository.UserExists(request.MessageObject.email)) throw new Exception("User already exists!");
        var salt = SecurityUtilities.GenerateSalt();
        var hash = SecurityUtilities.Hash(request.MessageObject.password!, salt);
        var user = chatRepository.InsertUser(request.MessageObject.email!, hash, salt);
        var jwt = SecurityUtilities.IssueJwt(
            new Dictionary<string, object?> { { "email", user.email }, { "id", user.id } });
        request.Socket.Authenticate(user);
        request.Socket.SendDto(new ServerAuthenticatesUser
        {
            jwt = jwt
        });
        return Task.CompletedTask;
    }
}