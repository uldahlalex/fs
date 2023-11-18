using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace core;

public class TransferObject
{
    public string eventType { get; set; }
    public object data { get; set; }
}

public class Message
{
    public int id { get; set; }
    public string messageContent { get; set; }
    public DateTimeOffset timestamp { get; set; }
    public int sender {get; set;}
    public int room { get; set; }
}

public class DownstreamSendPastMessagesForRoom
{
    public int roomId { get; set; }
    public IEnumerable<Message> messages { get; set; }
}

public class UpstreamEnterRoom
{
    public int roomId { get; set; }
}

public class EndUser
{
    public int id { get; set; }
    public string email { get; set; }
    public string hash { get; set; }
}

public class Room
{
    public int id {get; set; }
    public string title {get; set; }
}

public class UserRoomJunctions
{
    public int user { get; set; }
    public int room { get; set; }
}