using Newtonsoft.Json;

namespace Tests;

public class Helper
{
    public static void PrintObject(object o)
    {
        Console.WriteLine();
        var str = JsonConvert.SerializeObject(o);
        Console.WriteLine("OBJECT TYPE: {0}\n", o.GetType());
        Console.WriteLine("OBJECT JSON:");
        Console.WriteLine(str);
        Console.WriteLine();
    }
}