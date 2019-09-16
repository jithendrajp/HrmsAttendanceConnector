using RestSharp.Serializers;

namespace iCuboid.HRMS.DataConnector.Utils
{
    internal static class SerializeUtils
    {
        public static string ToString(object obj)
        {
            JsonSerializer s = new JsonSerializer();
            return s.Serialize(obj);
        }
    }
}