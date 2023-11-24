namespace core.Models;
using System;

public class Message
{
    public int id { get; set; }
    public string? messageContent { get; set; }
    public DateTimeOffset timestamp { get; set; }
    public int sender {get; set;}
    public int room { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public class EnforceNameAttribute : Attribute
{
    public string Name { get; }

    public EnforceNameAttribute(string name)
    {
        Name = name;
    }
}
public class EndUser
{    
    [EnforceName("id")]
    public int id { get; set; }
    public string? email { get; set; }
    public string? hash { get; set; }
    public string? salt { get; set; }
}

public class Room
{
    public int id {get; set; }
    public string? title {get; set; }
}

public class UserRoomJunctions
{
    public int user { get; set; }
    public int room { get; set; }
}