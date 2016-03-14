using System;
using System.Text;

namespace PervasiveDigital.Json
{
    public class JProperty : JToken
    {
        public JProperty()
        {
        }

        public JProperty(string name, JToken value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public JToken Value { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('"');
            sb.Append(this.Name);
            sb.Append("\" : ");
            sb.Append(this.Value.ToString());

            return sb.ToString();
        }
    }
}
