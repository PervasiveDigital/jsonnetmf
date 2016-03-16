using System;
using System.Text;
using System.Threading;

namespace PervasiveDigital.Json
{
    public class JToken
    {
        private bool _fOwnsContext;

        protected void EnterSerialization()
        {
            lock (JsonConverter.SyncObj)
            {
                if (JsonConverter.SerializationContext == null)
                {
                    JsonConverter.SerializationContext = new JsonConverter.SerializationCtx();
                    JsonConverter.SerializationContext.Indent = 0;
                    Monitor.Enter(JsonConverter.SerializationContext);
                    _fOwnsContext = true;
                }
            }
        }

        protected void ExitSerialization()
        {
            lock (JsonConverter.SyncObj)
            {
                if (_fOwnsContext)
                {
                    var monitorObj = JsonConverter.SerializationContext;
                    JsonConverter.SerializationContext = null;
                    _fOwnsContext = false;
                    Monitor.Exit(monitorObj);
                }
            }
        }

        protected string Indent(bool incrementAfter = false)
        {
            StringBuilder sb = new StringBuilder();
            string indent = "  ";
            if (JsonConverter.SerializationContext != null)
            {
                for (int i = 0; i < JsonConverter.SerializationContext.Indent; ++i)
                    sb.Append(indent);
                if (incrementAfter)
                    ++JsonConverter.SerializationContext.Indent;
            }
            return sb.ToString();
        }

        protected void Outdent()
        {
            --JsonConverter.SerializationContext.Indent;
        }
    }
}
