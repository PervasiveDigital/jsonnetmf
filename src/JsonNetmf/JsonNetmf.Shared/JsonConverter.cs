using System;
using System.Reflection;
using System.Text;

namespace PervasiveDigital.Json
{
    public static class JsonConverter
    {
        public static JToken Serialize(object oSource)
        {
            var type = oSource.GetType();
            if (type.IsArray)
                return JArray.Serialize(oSource);
            else
                return JObject.Serialize(type, oSource);
        }
    }
}
