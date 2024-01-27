namespace Externalities.ParameterModels;

public class IsMessageOwnerParams
{
    public int userId { get; set; }
    public int messageId { get; set; }

    public IsMessageOwnerParams(int userId, int messageId)
    {
        this.userId = userId;
        this.messageId = messageId;
    }
}