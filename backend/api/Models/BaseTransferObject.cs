namespace api.Models;

public class BaseTransferObject
{
    public BaseTransferObject()
    {
        var eventType = GetType().Name;
        var subString = eventType.Substring(eventType.Length-3);
        if (subString.ToLower().Equals("dto"))
        {
            this.eventType = eventType.Substring(0, eventType.Length - 3);
        }
        else
        {
            this.eventType = eventType;
        }
    }

    public string eventType { get; set; }
}