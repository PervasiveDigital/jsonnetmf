// (c) Pervasive Digital LLC
// Use of this code and resulting binaries is permitted only under the
// terms of a written license.
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
			set
			{
				if (name.ToLower() != value.Name.ToLower())
					throw new ArgumentException("index value must match property name");
				_members.Add(value.Name.ToLower(), value);
			}
		}

		public ICollection Members
		{
			get { return _members.Values; }
		}

		public void Add(string name, JToken value)
		{
			_members.Add(name.ToLower(), new JProperty(name, value));
		}

		public static JObject Serialize(Type type, object oSource)
		{
			var result = new JObject();
			var methods = type.GetMethods();
			foreach (var m in methods)
			{
				if (!m.IsPublic)
					continue;

				if (m.Name.IndexOf("get_") == 0)
				{
					var name = m.Name.Substring(4);
					var methodResult = m.Invoke(oSource, null);
					if (methodResult == null)
						result._members.Add(name.ToLower(), new JProperty(name, JValue.Serialize(m.ReturnType, null)));
					if (m.ReturnType.IsArray)
						result._members.Add(name.ToLower(), new JProperty(name, JArray.Serialize(m.ReturnType, methodResult)));
					else
						result._members.Add(name.ToLower(), new JProperty(name, JObject.Serialize(m.ReturnType, methodResult)));
				}
			}

			var fields = type.GetFields();
			foreach (var f in fields)
			{
				if (f.FieldType.IsNotPublic)
					continue;

				switch (f.MemberType)
				{
					case MemberTypes.Field:
					case MemberTypes.Property:
						var value = f.GetValue(oSource);
						if (value == null)
						{
							result._members.Add(f.Name, new JProperty(f.Name, JValue.Serialize(f.FieldType, null)));
						}
						else if (f.FieldType.IsValueType || f.FieldType == typeof(string))
						{
							result._members.Add(f.Name.ToLower(),
								new JProperty(f.Name, JValue.Serialize(f.FieldType, value)));
						}
						else
						{
							if (f.FieldType.IsArray)
							{
								result._members.Add(f.Name.ToLower(),
									new JProperty(f.Name, JArray.Serialize(f.FieldType, value)));
							}
							else
							{
								result._members.Add(f.Name.ToLower(),
									new JProperty(f.Name, JObject.Serialize(f.FieldType, value)));
							}
						}
						break;
					default:
						break;
				}
			}

			return result;
		}

		public override string ToString()
		{
			EnterSerialization();
			try
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine(Indent(true) + "{");
				bool first = true;
				foreach (var member in _members.Values)
				{
					if (!first)
						sb.AppendLine(",");
					first = false;
					sb.Append(Indent() + ((JProperty)member).ToString());
				}
				sb.AppendLine();
				Outdent();
				sb.Append(Indent() + "}");
				return sb.ToString();
			}
			finally
			{
				ExitSerialization();
			}
		}

		public override int GetBsonSize()
		{
			var result = 0;
			foreach (DictionaryEntry member in _members)
			{
				result += ((JProperty)member.Value).GetBsonSize(member.Key.ToString());
			}
			return result + 5;  // four bytes of size plus trailing nul
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

            foreach (DictionaryEntry member in _members)
            {
                ((JProperty)member.Value).ToBson(member.Key.ToString(), buffer, ref offset);
            }

            // Write the trailing nul
            buffer[offset++] = (byte)0;

            // Write the completed size
            SerializationUtilities.Marshall(buffer, ref startingOffset, offset);
		}

        public override BsonTypes GetBsonType()
        {
            return BsonTypes.BsonDocument;
        }

        internal static JObject FromBson(byte[] buffer, InstanceFactory factory = null)
        {
            throw new NotImplementedException();
        }

    }
}
