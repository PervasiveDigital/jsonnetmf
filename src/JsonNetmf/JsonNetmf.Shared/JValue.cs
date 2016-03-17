using System;
using System.Text;

namespace PervasiveDigital.Json
{
    public class JValue : JToken
    {
        public JValue()
        {
        }

        public JValue(object value)
        {
            this.Value = value;
        }

        public object Value { get; set; }

        public static JValue Serialize(Type type, object oValue)
        {
            return new JValue()
            {
                Value = oValue
            };
        }

        public override string ToString()
        {
            EnterSerialization();
            try
            {
                var type = this.Value.GetType();
                if (type == typeof(string) || type == typeof(char))
                    return "\"" + this.Value.ToString() + "\"";
                else if (type == typeof(DateTime))
                    return "\"" + ((DateTime)this.Value).ToString("r") + "\"";
                else
                    return this.Value.ToString();
            }
            finally
            {
                ExitSerialization();
            }
        }
    }
}
