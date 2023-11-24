namespace core.Models.WebsocketTransferObjects;

public class BaseTransferObject
{
    public string eventType { get; set; }

    public BaseTransferObject()
    {
        eventType = GetType().Name;
    }
}


