namespace Externalities._3rdPartyTransferModels;

public class ToxicityRequest
{
    public string text { get; set; }
    public List<string> categories { get; set; }
    public List<string> blocklistNames { get; set; }
    public bool haltOnBlocklistHit { get; set; }
    public string outputType { get; set; }
}

public class BlocklistsMatch
{
    public string blocklistName { get; set; }
    public string blocklistItemId { get; set; }
    public string blocklistItemText { get; set; }
}

public class CategoriesAnalysis
{
    public string category { get; set; }
    public int severity { get; set; }
}

public class ToxicityResponse
{
    public List<BlocklistsMatch> blocklistsMatch { get; set; }
    public List<CategoriesAnalysis> categoriesAnalysis { get; set; }
}