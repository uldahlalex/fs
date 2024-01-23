using System.Security.Authentication;
using api.Abstractions;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequireAuthenticationAttribute : BaseEventFilterAttribute
{
    public override Task Handle<T>(IWebSocketConnection socket, T dto)
    {
        if (!socket.IsAuthenticated())
            throw new AuthenticationException("Client is not authenticated!");
        return Task.CompletedTask;
    }
}