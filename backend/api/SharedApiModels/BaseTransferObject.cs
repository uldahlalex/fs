namespace api;

public class BaseTransferObject
{
    public BaseTransferObject()
    {
        eventType = GetType().Name;
    }

    public string eventType { get; set; }
}