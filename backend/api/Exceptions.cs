namespace api;

internal class EventNotFoundException : Exception
{
    public EventNotFoundException(string notFound)
    {
        throw new NotImplementedException();
    }
}

public class DeserializationException : Exception
{
    public DeserializationException()
    {
    }

    public DeserializationException(string message)
        : base(message)
    {
    }

    public DeserializationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}