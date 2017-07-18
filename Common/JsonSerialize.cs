using Newtonsoft.Json;

namespace Common
{
    public class JsonSerialize
    {
        public static T DeSerialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string EnSerialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
