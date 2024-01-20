using System.Security.Authentication;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequireAuthenticationAttribute : Attribute
{
    public static void ValidateAuthentication(IWebSocketConnection socket)
    {
        if (!socket.IsAuthenticated())
            throw new AuthenticationException("Client is not authenticated!");
    }
}