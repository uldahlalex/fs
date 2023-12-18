namespace api.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RequireAuthenticationAttribute : Attribute;
//Invoked in BaseEventHandler.cs