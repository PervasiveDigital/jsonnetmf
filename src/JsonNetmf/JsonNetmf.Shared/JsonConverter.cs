using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.SPOT;

namespace PervasiveDigital.Json
{
    public static class JsonConverter
    {
        private enum TokenType
        {
            LBrace, RBrace, LArray, RArray, Colon, Comma, String, Number, Date, Error, End
        }

        private struct LexToken
        {
            public TokenType TType;
            public string TValue;
        }

        public class SerializationCtx
        {
            public int Indent;
        }

        public static SerializationCtx SerializationContext = null;
        public static object SyncObj = new object();

        public static JToken Serialize(object oSource)
        {
            var type = oSource.GetType();
            if (type.IsArray)
                return JArray.Serialize(oSource);
            else
                return JObject.Serialize(type, oSource);
        }

        public static JToken Deserialize(string sourceString)
        {
            var data = Encoding.UTF8.GetBytes(sourceString);
            var mem = new MemoryStream(data);
            mem.Seek(0, SeekOrigin.Begin);
            return Deserialize(new StreamReader(mem));
        }

        public static JToken Deserialize(Stream sourceStream)
        {
            return Deserialize(new StreamReader(sourceStream));
        }

        public static JToken Deserialize(StreamReader sourceReader)
        {
            LexToken token;
            do
            {
                token = GetNextToken(sourceReader);
                Debug.Print(token.TType.ToString());
                if (token.TType == TokenType.String || token.TType == TokenType.Number)
                    Debug.Print("    " + token.TValue);
            } while (token.TType != TokenType.End);
            return null;
        }

        private static LexToken GetNextToken(StreamReader sourceReader)
        {
            StringBuilder sb = null;

            while (true) // EndOfStream doesn't seem to work for mem streams
            {
                char ch = (char) sourceReader.Read();
                if (ch != '"' && sb != null)
                {
                    sb.Append(ch);
                }
                else
                {
                    if (IsNumberChar(ch))
                    {
                        sb = new StringBuilder();
                        while (IsNumberChar(ch))
                        {
                            sb.Append(ch);
                            ch = (char)sourceReader.Read();
                        }
                        // Note that we don't claim that this is a well-formed number
                        return new LexToken() {TType = TokenType.Number, TValue = sb.ToString()};
                    }
                    switch (ch)
                    {
                        case '{':
                            return new LexToken() {TType = TokenType.LBrace, TValue = null};
                        case '}':
                            return new LexToken() {TType = TokenType.RBrace, TValue = null};
                        case '[':
                            return new LexToken() {TType = TokenType.LArray, TValue = null};
                        case ']':
                            return new LexToken() {TType = TokenType.RArray, TValue = null};
                        case ':':
                            return new LexToken() { TType = TokenType.Colon, TValue = null };
                        case ',':
                            return new LexToken() { TType = TokenType.Comma, TValue = null };
                        case '"':
                            if (sb == null)
                            {
                                sb = new StringBuilder();
                            }
                            else
                            {
                                return new LexToken() {TType = TokenType.String, TValue = sb.ToString()};
                            }
                            break;
                        case ' ':
                        case '\t':
                        case '\r':
                        case '\n':
                            break; // go around again
                        case (char)0xffff:
                            return new LexToken() {TType = TokenType.End, TValue = null};
                        default:
                            break;
                    }
                }
            }
            if (sb!=null)
                return new LexToken() { TType = TokenType.Error, TValue = null };
            else
                return new LexToken() { TType = TokenType.End, TValue = null };
        }

        private static bool IsNumberChar(char ch)
        {
            return (ch == '-') || (ch == '+') || (ch == '.') || (ch=='e') || (ch=='E') || (ch >= '0' & ch <= '9');
        }
    }
}
