using System.Text.Json;

namespace ByteAwesome.TestAPI.Data;

public class DataList<TData>
{
    public static List<TData> JsonList;
    public static void LoadData(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            JsonList =  JsonSerializer.Deserialize<List<TData>>(json) ?? new List<TData>();
        }
    }
}

public class CountryCurrencyList : DataList<CountryCurrency>
{
}