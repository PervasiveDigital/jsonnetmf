using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace PervasiveDigital.Json
{
    public class JObject : JToken
    {
        private readonly Hashtable _members = new Hashtable();

        public JProperty this[string name]
        {
            get { return (JProperty)_members[name.ToLower()]; }
        }

        public static JObject Serialize(Type type, object oSource)
        {
            var result = new JObject();

            var fields = type.GetFields();
            foreach (var f in fields)
            {
                switch (f.MemberType)
                {
                    case MemberTypes.Field:
                    case MemberTypes.Property:
                        if (f.FieldType.IsValueType)
                            result._members.Add(f.Name.ToLower(), new JProperty(f.Name, JValue.Serialize(f.FieldType, f.GetValue(oSource))));
                        else
                            result._members.Add(f.Name.ToLower(), new JProperty(f.Name, JObject.Serialize(f.FieldType, f.GetValue(oSource))));
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('{');
            foreach (var member in _members.Values)
            {
                sb.Append(((JProperty) member).ToString());
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
