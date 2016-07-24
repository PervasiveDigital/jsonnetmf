// (c) Pervasive Digital LLC
// Use of this code and resulting binaries is permitted only under the
// terms of a written license.
using System;
using System.Text;

namespace PervasiveDigital.Json
{
	public class JArray : JToken
	{
		private readonly JToken[] _contents;

		public JArray()
		{
		}

		public JArray(JToken[] values)
		{
			_contents = values;
		}

		private JArray(Array source)
		{
			_contents = new JToken[source.Length];
			for (int i = 0; i < source.Length; ++i)
			{
				var value = source.GetValue(i);
				var fieldType = value.GetType();

				if (value == null)
				{
					_contents[i] = JValue.Serialize(fieldType, null);
				}
				else if (fieldType.IsValueType || fieldType == typeof(string))
				{
					_contents[i] = JValue.Serialize(fieldType, value);
				}
				else
				{
					if (fieldType.IsArray)
					{
						_contents[i] = JArray.Serialize(fieldType, value);
					}
					else
					{
						_contents[i] = JObject.Serialize(fieldType, value); ;
					}
				}

			}
		}

		public int Length
		{
			get { return _contents.Length; }
		}

		public JToken[] Items
		{
			get { return _contents; }
		}

		public static JArray Serialize(Type type, object oSource)
		{
			return new JArray((Array)oSource);
		}

		public JToken this[int i]
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

		public override int GetBsonSize()
		{
            int offset = 0;
            this.ToBson(null, ref offset);
            return offset;
        }

        public override int GetBsonSize(string ename)
		{
            return 1 + ename.Length + 1 + this.GetBsonSize();
        }

        public override void ToBson(byte[] buffer, ref int offset)
		{
            int startingOffset = offset;

            // leave space for the size
            offset += 4;

            for (int i = 0; i < _contents.Length; ++i)
            {
                _contents[i].ToBson(i.ToString(), buffer, ref offset);
            }

            // Write the trailing nul
            if (buffer!=null)
                buffer[offset] = (byte)0;
            ++offset;

            // Write the completed size
            if (buffer!=null)
                SerializationUtilities.Marshall(buffer, ref startingOffset, offset);
        }

        public override BsonTypes GetBsonType()
        {
            return BsonTypes.BsonArray;
        }

        internal static JArray FromBson(byte[] buffer, ref int offset, InstanceFactory factory = null)
        {
            throw new NotImplementedException();
        }
    }
}
