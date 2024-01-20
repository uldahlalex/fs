namespace api.Externalities;

public class GetPastMessagesParams
{
    public GetPastMessagesParams(int room, int lastMessageId = Int32.MaxValue)
    {
        this.room = room;
        this.lastMessageId = lastMessageId;
    }

    public int room { get; private set; }
    public int lastMessageId { get; private set; }
}