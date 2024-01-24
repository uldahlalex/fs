namespace Externalities.QueryModels;

public class MessageWithSenderEmail
{
    public int id { get; set; }
    public DateTimeOffset timestamp { get; set; }
    public string? messageContent { get; set; }
    public int sender { get; set; }
    public int room { get; set; }
    public string? email { get; set; }
}