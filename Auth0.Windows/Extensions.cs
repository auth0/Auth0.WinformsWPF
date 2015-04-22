using Newtonsoft.Json.Linq;

namespace Auth0.Windows
{
    internal static class Extensions
    {
        internal static JObject ToJson(this string jsonString)
        {
            return JObject.Parse(jsonString);
        }
    }
}