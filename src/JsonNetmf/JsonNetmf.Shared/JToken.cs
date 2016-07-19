// (c) Pervasive Digital LLC
// Use of this code and resulting binaries is permitted only under the
// terms of a written license.
using System;
using System.Text;
using System.Threading;

namespace PervasiveDigital.Json
{
	public abstract class JToken
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

		public byte[] ToBson()
		{
			var size = this.GetBsonSize("") + 5;
			var buffer = new byte[size];
			int offset = 4;
            this.ToBson("", buffer, ref offset);

            // Write the trailing nul
            buffer[offset++] = (byte)0;

            // Write the completed size
            int zero = 0;
            SerializationUtilities.Marshall(buffer, ref zero, offset);
            return buffer;
		}

        public abstract BsonTypes GetBsonType();

		public abstract int GetBsonSize();

		public abstract int GetBsonSize(string ename);

		public abstract void ToBson(byte[] buffer, ref int offset);

        public void ToBson(string ename, byte[] buffer, ref int offset)
        {
#if DEBUG
            int startingOffset = offset;
#endif

            buffer[offset++] = (byte)this.GetBsonType();
            MarshallEName(ename, buffer, ref offset);
            ToBson(buffer, ref offset);

#if DEBUG
            if (this.GetBsonSize(ename) != (offset - startingOffset))
                throw new Exception("marshalling error");
#endif
        }

        protected void MarshallEName(string ename, byte[] buffer, ref int offset)
        {
            var name = Encoding.UTF8.GetBytes(ename);
            Array.Copy(name, 0, buffer, offset, name.Length);
            offset += name.Length;
            buffer[offset++] = 0;
        }

    }
}
