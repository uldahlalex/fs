namespace Infrastructure.DbModels;

public class Room
{
    [EnforceName("id")] public int id { get; set; }

    [EnforceName("title")] public string? title { get; set; }
}