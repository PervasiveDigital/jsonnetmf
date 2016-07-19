// (c) Pervasive Digital LLC
// Use of this code and resulting binaries is permitted only under the
// terms of a written license.
using System;

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
				if (Value == null)
					return "null";

				var type = this.Value.GetType();
				if (type == typeof(string) || type == typeof(char))
					return "\"" + this.Value.ToString() + "\"";
				else if (type == typeof(DateTime))
					return "\"" + DateTimeExtensions.ToIso8601(((DateTime)this.Value)) + "\"";
				else
					return this.Value.ToString();
			}
			finally
			{
				ExitSerialization();
			}
		}

		public override int GetBsonSize()
		{
			if (this.Value == null)
				return 0;

			var type = this.Value.GetType();
			if (type == typeof(double))
				return 8;
			else if (type == typeof(string))
				return ((string)this.Value).Length + 1;  // preamble, strlen, nul
			if (type == typeof(char))
				return 1 + 1;  // strlen==1, nul
			else if (type == typeof(Int32) || type == typeof(UInt32))
				return 4;
			else if (type == typeof(Int64) || type == typeof(UInt64))
				return 8;
			else if (type == typeof(DateTime))
				return 8;
			else if (type == typeof(bool))
				return 1;
			else
				throw new Exception("Unsupported type");
		}

		public override int GetBsonSize(string ename)
		{
			return 1 + ename.Length + 1 + this.GetBsonSize();
		}

		public override void ToBson(byte[] buffer, ref int offset)
		{
			if (this.Value != null)
				SerializationUtilities.Marshall(buffer, ref offset, this.Value);
		}

        public override BsonTypes GetBsonType()
        {
            if (this.Value == null)
                return BsonTypes.BsonNull;

            var type = this.Value.GetType();
            if (type == typeof(double))
                return BsonTypes.BsonDouble;
            else if (type == typeof(string))
                return BsonTypes.BsonString;
            if (type == typeof(char))
                return BsonTypes.BsonString;
            else if (type == typeof(Int32) || type == typeof(UInt32))
                return BsonTypes.BsonInt32;
            else if (type == typeof(Int64) || type == typeof(UInt64))
                return BsonTypes.BsonInt64;
            else if (type == typeof(DateTime))
                return BsonTypes.BsonDateTime;
            else if (type == typeof(bool))
                return BsonTypes.BsonBoolean;
            else
                throw new Exception("Unsupported type");
        }
    }
}
