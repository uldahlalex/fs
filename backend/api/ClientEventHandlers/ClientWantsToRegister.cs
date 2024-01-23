using System.ComponentModel.DataAnnotations;
using api.Abstractions;
using api.Attributes.EventFilters;
using api.Externalities;
using api.Models;
using api.Models.ServerEvents;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToRegisterDto : BaseDto
{
    [EmailAddress] public string? email { get; set; }

    [MinLength(6)] public string? password { get; set; }
}

[RateLimit(1, 60)]
public class ClientWantsToRegister(ChatRepository chatRepository) : BaseEventHandler<ClientWantsToRegisterDto>
{
    public override async Task Handle(ClientWantsToRegisterDto dto, IWebSocketConnection socket)
    {
        if (await chatRepository.DoesUserAlreadyExist(new FindByEmailParams(dto.email!))) throw new Exception("User already exists!");
        var salt = SecurityUtilities.GenerateSalt();
        var hash = SecurityUtilities.Hash(dto.password!, salt);
        var user = await chatRepository.InsertUser(new InsertUserParams(dto.email!, hash, salt));
        var jwt = SecurityUtilities.IssueJwt(user);
        socket.Authenticate(user);
        socket.SendDto(new ServerAuthenticatesUser
        {
            jwt = jwt
        });
    }
}