using api.Helpers.Attributes;

namespace api.Models.DbModels;

public class EndUser
{
    [EnforceName("id")] public int id { get; set; }

    [EnforceName("email")] public string? email { get; set; }

    [EnforceName("hash")] public string? hash { get; set; }

    [EnforceName("salt")] public string? salt { get; set; }
    [EnforceName("isbanned")] public bool isbanned { get; set; }
}