using System.Security.Authentication;
using core.Exceptions;
using core.ExtensionMethods;
using core.Models.DbModels;
using core.Models.WebsocketTransferObjects;
using core.SecurityUtilities;
using Fleck;
using Infrastructure;
using MediatR;
using Serilog;
using System.Text.Json;


namespace api;




public class EventTypeRequest<T> : IRequest
{
    public IWebSocketConnection Socket { get; set; }
    public T Dto { get; set; }
}

public class WebSocketRequest : IRequest
{
    public IWebSocketConnection Socket { get; set; }
    public string RawMessage { get; set; }
}


public class ClientWantsToAuthenticateHandler
    : IRequestHandler<WebSocketRequest>
{
    private readonly ChatRepository _chatRepository;

    public ClientWantsToAuthenticateHandler(ChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }
    
    public Task Handle(WebSocketRequest request, CancellationToken cancellationToken)
    {
        var dto = request.RawMessage.DeserializeToModelAndValidate<core.Models.WebsocketTransferObjects.ClientWantsToAuthenticate>();
        EndUser user;
        try
        {
            user =  _chatRepository.GetUser(dto.email!);
        }
        catch (Exception e)
        {
            Log.Error(e, "WebsocketServer");
            throw new AuthenticationException("User does not exist!");
        }

        var expectedHash = SecurityUtilities.Hash(dto.password, user.salt!);
        if (!expectedHash.Equals(user.hash)) throw new AuthenticationException("Wrong password!");
        var jwt = SecurityUtilities.IssueJwt(new Dictionary<string, object?>
            { { "email", user.email }, { "id", user.id } });
        request.Socket.Authenticate(user);
        request.Socket.SendDto(new ServerAuthenticatesUser { jwt = jwt });

        return Task.FromResult(Unit.Value);
    }
    
    private T Deserialize<T>(string message)
    {
        return JsonSerializer.Deserialize<T>(message,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new DeserializationException($"Failed to deserialize message: {message}");
    }
}