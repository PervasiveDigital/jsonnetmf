using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.SPOT;

namespace PervasiveDigital.Json
{
    // The protocol mantra: Be strict in what you emit, and generous in what you accept.

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
                return JArray.Serialize(type, oSource);
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
            JToken result = null;
            LexToken token;
            token = GetNextToken(sourceReader);
            switch (token.TType)
            {
                case TokenType.LBrace:
                    result = ParseObject(sourceReader, ref token);
                    if (token.TType == TokenType.RBrace)
                        token = GetNextToken(sourceReader);
                    else if (token.TType != TokenType.End && token.TType != TokenType.Error)
                        throw new Exception("unexpected content after end of object");
                    break;
                case TokenType.LArray:
                    result = ParseArray(sourceReader, ref token);
                    if (token.TType == TokenType.RArray)
                        token = GetNextToken(sourceReader);
                    else if (token.TType != TokenType.End && token.TType != TokenType.Error)
                        throw new Exception("unexpected content after end of array");
                    break;
                default:
                    throw new Exception("unexpected initial token in json parse");
            }
            if (token.TType != TokenType.End)
                throw new Exception("unexpected end token in json parse");
            else if (token.TType == TokenType.Error)
                throw new Exception("unexpected lexical token during json parse");
            return result;
        }

        private static JObject ParseObject(StreamReader sr, ref LexToken token)
        {
            Debug.Assert(token.TType == TokenType.LBrace);
            var result = new JObject();
            token = GetNextToken(sr);
            do
            {
                if (token.TType != TokenType.String)
                    throw new Exception("expected label");
                var propName = token.TValue;

                token = GetNextToken(sr);
                if (token.TType != TokenType.Colon)
                    throw new Exception("expected colon");

                var value = ParseValue(sr, ref token);
                result.Add(propName, value);

                token = GetNextToken(sr);
                if (token.TType == TokenType.Comma)
                    token = GetNextToken(sr);

            } while (token.TType != TokenType.End && token.TType != TokenType.Error && token.TType != TokenType.RBrace);
            if (token.TType == TokenType.Error)
                throw new Exception("unexpected token in json object");
            else if (token.TType!=TokenType.RBrace)
                throw new Exception("unterminated json object");
            return result;
        }

        private static JArray ParseArray(StreamReader sr, ref LexToken token)
        {
            Debug.Assert(token.TType == TokenType.LArray);
            ArrayList list = new ArrayList();
            do
            {
                var value = ParseValue(sr, ref token);
                list.Add(value);
                token = GetNextToken(sr);
            } while (token.TType != TokenType.End && token.TType != TokenType.Error && token.TType != TokenType.RArray);
            if (token.TType==TokenType.Error)
                throw new Exception("unexpected token in array");
            else if (token.TType != TokenType.RArray)
                throw new Exception("unterminated json array");
            var result = new JArray((JValue[])list.ToArray(typeof(JValue)));
            return result;
        }
        private static JToken ParseValue(StreamReader sr, ref LexToken token)
        {
            token = GetNextToken(sr);
            if (token.TType == TokenType.String)
                return new JValue(token.TValue);
            else if (token.TType == TokenType.Number)
            {
                if (token.TValue.IndexOfAny(new char[] {'.','e','E'})!=-1)
                    return new JValue(double.Parse(token.TValue));
                else
                    return new JValue(int.Parse(token.TValue));
            }
            else if (token.TType == TokenType.Date)
            {
                throw new NotSupportedException("datetime parsing not supported");
            }
            else if (token.TType == TokenType.LBrace)
                return ParseObject(sr, ref token);
            else if (token.TType == TokenType.LArray)
                return ParseArray(sr, ref token);

            throw new Exception("invalid value found during json parse");
        }

        private static LexToken GetNextToken(StreamReader sourceReader)
#if DEBUG
        {
            // a diagnostic wrapper, used only in debug builds
            var result = GetNextTokenInternal(sourceReader);
            Debug.Print(result.TType.TokenTypeToString());
            if (result.TType == TokenType.String || result.TType == TokenType.Number)
                Debug.Print("    " + result.TValue);
            return result;
        }

        private static LexToken GetNextTokenInternal(StreamReader sourceReader)
#endif
        {
            StringBuilder sb = null;
            char openQuote = '\0';

            while (true) // EndOfStream doesn't seem to work for mem streams - it's always 'true'
            {
                char ch = (char)sourceReader.Read();

                // Handle json escapes
                bool escaped = false;
                if (ch == '\\')
                {
                    escaped = true;
                    ch = (char)sourceReader.Read();
                    if (ch == (char)0xffff)
                        return EndToken(sb);
                    //TODO: replace with a mapping array? This switch is really incomplete.
                    switch (ch)
                    {
                        case '\'':
                            ch = '\'';
                            break;
                        case '"':
                            ch = '"';
                            break;
                        case 't':
                            ch = '\t';
                            break;
                        case 'r':
                            ch = '\r';
                            break;
                        case 'n':
                            ch = '\n';
                            break;
                        default:
                            throw new Exception("unsupported escape");
                    }
                }

                if (sb != null && (ch != openQuote || escaped))
                {
                    sb.Append(ch);
                }
                else if (IsNumberIntroChar(ch))
                {
                    sb = new StringBuilder();
                    while (IsNumberChar(ch))
                    {
                        sb.Append(ch);
                        // Don't consume chars that are not part of the number
                        ch = (char)sourceReader.Peek();
                        if (IsNumberChar(ch))
                            sourceReader.Read();

                        if (ch == (char)0xffff)
                            return EndToken(sb);
                    }
                    // Note that we don't claim that this is a well-formed number
                    return new LexToken() { TType = TokenType.Number, TValue = sb.ToString() };
                }
                else
                {
                    switch (ch)
                    {
                        case '{':
                            return new LexToken() { TType = TokenType.LBrace, TValue = null };
                        case '}':
                            return new LexToken() { TType = TokenType.RBrace, TValue = null };
                        case '[':
                            return new LexToken() { TType = TokenType.LArray, TValue = null };
                        case ']':
                            return new LexToken() { TType = TokenType.RArray, TValue = null };
                        case ':':
                            return new LexToken() { TType = TokenType.Colon, TValue = null };
                        case ',':
                            return new LexToken() { TType = TokenType.Comma, TValue = null };
                        case '"':
                        case '\'':
                            if (sb == null)
                            {
                                openQuote = ch;
                                sb = new StringBuilder();
                            }
                            else
                            {
                                // We're building a string and we hit a quote character.
                                // The ch must match openQuote, or otherwise we should have eaten it above as string content
                                Debug.Assert(ch == openQuote);
                                return new LexToken() { TType = TokenType.String, TValue = sb.ToString() };
                            }
                            break;
                        case ' ':
                        case '\t':
                        case '\r':
                        case '\n':
                            break; // whitespace - go around again
                        case (char)0xffff:
                            return EndToken(sb);
                        default:
                            throw new Exception("unexpected character during json lexical parse");
                    }
                }
            }
            return EndToken(sb);
        }

        private static LexToken EndToken(StringBuilder sb)
        {
            if (sb != null)
                return new LexToken() { TType = TokenType.Error, TValue = null };
            else
                return new LexToken() { TType = TokenType.End, TValue = null };
        }

        // Legal first characters for numbers
        private static bool IsNumberIntroChar(char ch)
        {
            return (ch == '-') || (ch == '+') || (ch == '.') || (ch >= '0' & ch <= '9');
        }

        // Legal chars for 2..n'th position of a number
        private static bool IsNumberChar(char ch)
        {
            return (ch == '-') || (ch == '+') || (ch == '.') || (ch == 'e') || (ch == 'E') || (ch >= '0' & ch <= '9');
        }

#if DEBUG
        private static string TokenTypeToString(this TokenType val)
        {
            switch (val)
            {
                case TokenType.Colon:
                    return "COLON";
                case TokenType.Comma:
                    return "COMMA";
                case TokenType.Date:
                    return "DATE";
                case TokenType.End:
                    return "END";
                case TokenType.Error:
                    return "ERROR";
                case TokenType.LArray:
                    return "LARRAY";
                case TokenType.LBrace:
                    return "LBRACE";
                case TokenType.Number:
                    return "NUMBER";
                case TokenType.RArray:
                    return "RARRAY";
                case TokenType.RBrace:
                    return "RBRACE";
                case TokenType.String:
                    return "STRING";
                default:
                    return "??unknown??";
            }
        }
#endif
    }
}
