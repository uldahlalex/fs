namespace Externalities.ParameterModels;

public class IsMessageOwnerParams
{
    public IsMessageOwnerParams(int userId, int messageId)
    {
        this.userId = userId;
        this.messageId = messageId;
    }

    public int userId { get; set; }
    public int messageId { get; set; }
}