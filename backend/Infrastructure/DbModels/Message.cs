namespace Infrastructure.DbModels;

public class Message
{
    [EnforceName("id")] public int id { get; set; }

    [EnforceName("messageContent")] public string? messageContent { get; set; }

    [EnforceName("timestamp")] public DateTimeOffset timestamp { get; set; }

    [EnforceName("sender")] public int sender { get; set; }

    [EnforceName("room")] public int room { get; set; }
}