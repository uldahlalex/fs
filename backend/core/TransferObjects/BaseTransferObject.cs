namespace core;

public class BaseTransferObject
{
    public string eventType { get; set; }

    public BaseTransferObject()
    {
        eventType = GetType().Name;
    }
}
