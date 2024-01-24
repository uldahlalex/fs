namespace Externalities.QueryModels;

public class Message
{
    public int id { get; set; }

    /// <summary>
    ///     Column name in db: messagecontent
    /// </summary>
    public string? messageContent { get; set; }

    public DateTimeOffset timestamp { get; set; }
    public int sender { get; set; }
    public int room { get; set; }
}