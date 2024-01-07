namespace api.Models.DbModels;

public class Message
{
    public int id { get; set; }
    public string? messageContent { get; set; }
    public DateTimeOffset timestamp { get; set; }
    public int sender { get; set; }
    public int room { get; set; }
}