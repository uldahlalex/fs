using core.Attributes;

namespace core.Models.DbModels;

public class Room
{
    [EnforceName("id")] public int id { get; set; }

    [EnforceName("title")] public string? title { get; set; }
}