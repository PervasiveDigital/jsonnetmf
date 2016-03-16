using System;
using System.Reflection;
using System.Text;
using System.Threading;

namespace PervasiveDigital.Json
{
    public static class JsonConverter
    {
        public class SerializationCtx
        {
            public int Indent;
        }

        public static SerializationCtx SerializationContext = null;
        public static object SyncObj = new object();

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
