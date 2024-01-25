namespace Externalities.QueryModels;

public class EndUser
{
    public int id { get; set; }
    public string? email { get; set; }
    public string? hash { get; set; }
    public string? salt { get; set; }
    public bool isbanned { get; set; }
    public bool isadmin { get; set; }
}