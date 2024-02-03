using System.Security.Authentication;
using api.StaticHelpers.ExtensionMethods;
using Fleck;
using lib;

namespace api.Attributes.EventFilters;

public class RequireAuthenticationAttribute : BaseEventFilter
{
    public override Task Handle<T>(IWebSocketConnection socket, T dto)
    {
        if (!socket.IsAuthenticated())
            throw new AuthenticationException("Client is not authenticated!");
        return Task.CompletedTask;
    }
}