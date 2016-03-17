using System;
using System.Text;

namespace PervasiveDigital.Json
{
    public class JArray : JToken
    {
        private readonly JValue[] _contents;

        public JArray()
        {
        }

        public JArray(JValue[] values)
        {
            _contents = values;
        }

        private JArray(Array source)
        {
            _contents = new JValue[source.Length];
            for (int i = 0 ; i<source.Length; ++i)
            {
                _contents[i] = new JValue(source.GetValue(i));
            }
        }

        public int Length
        {
            get { return _contents.Length; }
        }

        public JValue[] Items
        {
            get {  return _contents; }
        }

        public static JArray Serialize(Type type, object oSource)
        {
            return new JArray((Array)oSource);
        }

        public JValue this[int i]
        {
            get { return _contents[i]; }
        }

        public override string ToString()
        {
            EnterSerialization();
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append('[');
                Indent(true);
                int prefaceLength = 0;

                bool first = true;
                foreach (var item in _contents)
                {
                    if (!first)
                    {
                        if (sb.Length - prefaceLength > 72)
                        {
                            sb.AppendLine(",");
                            prefaceLength = sb.Length;
                        }
                        else
                        {
                            sb.Append(',');
                        }
                    }
                    first = false;
                    sb.Append(item);
                }
                sb.Append(']');
                Outdent();
                return sb.ToString();
            }
            finally
            {
                ExitSerialization();
            }
        }
    }
}
