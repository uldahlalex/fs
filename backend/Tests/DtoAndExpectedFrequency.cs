using api.Models;

namespace Tests;

public class DtoAndExpectedFrequency
{
    public Type EventType { get; set; }
    public int Occurrences { get; set; }
    
    public DtoAndExpectedFrequency(Type eventType, int occurrences = 1)
    {
        EventType = eventType;
        Occurrences = occurrences;
    }

}
