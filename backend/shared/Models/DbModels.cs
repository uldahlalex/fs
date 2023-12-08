using core.Attributes;

namespace core.Models;

public class Message
{
    [EnforceName("id")] public int id { get; set; }

    [EnforceName("messageContent")] public string? messageContent { get; set; }

    [EnforceName("timestamp")] public DateTimeOffset timestamp { get; set; }

    [EnforceName("sender")] public int sender { get; set; }

    [EnforceName("room")] public int room { get; set; }
}

public class EndUser
{
    [EnforceName("id")] public int id { get; set; }

    [EnforceName("email")] public string? email { get; set; }

    [EnforceName("hash")] public string? hash { get; set; }

    [EnforceName("salt")] public string? salt { get; set; }
    [EnforceName("isbanned")] public bool isbanned { get; set; }
}

public class Room
{
    [EnforceName("id")] public int id { get; set; }

    [EnforceName("title")] public string? title { get; set; }
}

public class UserRoomJunctions
{
    [EnforceName("user")] public int user { get; set; }

    [EnforceName("room")] public int room { get; set; }
}

public class TimeSeries
{
    [EnforceName("id")] public int id { get; set; }

    [EnforceName("datapoint")] public int? datapoint { get; set; }

    [EnforceName("timestamp")] public DateTimeOffset timestamp { get; set; }
}

public class MessageWithSenderEmail //Situational query model
{
    public int id { get; set; }
    public DateTimeOffset timestamp { get; set; }
    public string? messageContent { get; set; }
    public int sender { get; set; }
    public int room { get; set; }
    public string? email { get; set; }
}