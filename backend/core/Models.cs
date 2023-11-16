using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace core;

public class TransferObject
{
    public string Action { get; set; }
    public JObject Data { get; set; }
}

public class Message
{
    public int id { get; set; }
    public string messageContent { get; set; }
    public DateTimeOffset timestamp { get; set; }
    public int sender {get; set;}
    public int room { get; set; }
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