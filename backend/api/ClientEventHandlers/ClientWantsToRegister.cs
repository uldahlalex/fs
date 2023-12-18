using System.ComponentModel.DataAnnotations;
using api.Abstractions;
using api.Externalities;
using api.Helpers;
using api.Helpers.ExtensionMethods;
using api.Models;
using api.Models.ServerEvents;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToRegisterDto : BaseTransferObject
{
    [EmailAddress] public string? email { get; set; }

    [MinLength(6)] public string? password { get; set; }
}

public class  ClientWantsToRegister(ChatRepository chatRepository) : BaseEventHandler<ClientWantsToRegisterDto>
{
    public override Task Handle(ClientWantsToRegisterDto dto, IWebSocketConnection socket)
    {
        if (chatRepository.UserExists(dto.email)) throw new Exception("User already exists!");
        var salt = SecurityUtilities.GenerateSalt();
        var hash = SecurityUtilities.Hash(dto.password!, salt);
        var user = chatRepository.InsertUser(dto.email!, hash, salt);
        var jwt = SecurityUtilities.IssueJwt(
            new Dictionary<string, object?> { { "email", user.email }, { "id", user.id } });
        socket.Authenticate(user);
        socket.SendDto(new ServerAuthenticatesUser
        {
            jwt = jwt
        });
        return Task.CompletedTask;
    }
}