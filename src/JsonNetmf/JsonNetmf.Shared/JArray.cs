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
			throw new NotImplementedException();
		}

		public override int GetBsonSize(string ename)
		{
			throw new NotImplementedException();
		}

		public override void ToBson(byte[] buffer, ref int offset)
		{
			throw new NotImplementedException();
		}

        public override BsonTypes GetBsonType()
        {
            return BsonTypes.BsonArray;
        }

        internal static JArray FromBson(byte[] buffer, InstanceFactory factory = null)
        {
            throw new NotImplementedException();
        }
    }
}
