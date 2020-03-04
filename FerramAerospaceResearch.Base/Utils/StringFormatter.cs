using System;
using System.Collections.Generic;
using System.Text;
using FerramAerospaceResearch.Interfaces;

namespace FerramAerospaceResearch
{
    public class StringFormatter : IConfigValue<string>
    {
        private const string LeftBrace = "<<<";
        private const string RightBrace = ">>>";

        private readonly List<Field> fields = new List<Field>();
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private string formatString;

        public StringFormatter(string formatString)
        {
            Parse(formatString);
        }

        public string FormatString
        {
            get { return formatString; }
            set { Parse(value); }
        }

        public void Set(object value)
        {
            if (value is string str)
                Set(str);
        }

        public string Get()
        {
            return FormatString;
        }

        public void Set(string value)
        {
            FormatString = value;
        }

        object IConfigValue.Get()
        {
            return Get();
        }

        public void Parse(string str)
        {
            fields.Clear();
            formatString = str;

            int begin = 0;
            while (begin < str.Length)
            {
                int replaceBegin = str.IndexOf(LeftBrace, begin, StringComparison.Ordinal);

                // no replacements found
                if (replaceBegin < 0)
                {
                    fields.Add(new Field
                    {
                        Type = FieldType.Text,
                        Str = str.Substring(begin, str.Length - begin)
                    });
                    break;
                }

                int paramBegin = replaceBegin + LeftBrace.Length;
                int paramEnd = str.IndexOf(RightBrace, paramBegin, StringComparison.Ordinal);
                // no closing brace found
                if (paramEnd < 0)
                {
                    fields.Add(new Field
                    {
                        Type = FieldType.Text,
                        Str = str.Substring(begin, str.Length - begin)
                    });
                    break;
                }

                // found braced parameter
                fields.Add(new Field
                {
                    Type = FieldType.Text,
                    Str = str.Substring(begin, replaceBegin - begin)
                });

                fields.Add(new Field
                {
                    Type = FieldType.Replacement,
                    Str = str.Substring(paramBegin, paramEnd - paramBegin)
                });

                begin = paramEnd + RightBrace.Length;
            }
        }

        public string ToString(StringBuilder sb, Dictionary<string, object> replacements)
        {
            sb.Clear();
            foreach (Field field in fields)
            {
                if (field.Type == FieldType.Text)
                    sb.Append(field.Str);
                else if (replacements.TryGetValue(field.Str, out object o))
                    sb.Append(o.ToString());
                else
                {
                    sb.Append(LeftBrace);
                    sb.Append(field.Str);
                    sb.Append(RightBrace);
                }
            }

            return sb.ToString();
        }

        public string ToString(Dictionary<string, object> replacements)
        {
            return ToString(stringBuilder, replacements);
        }

        public override string ToString()
        {
            return FormatString;
        }

        private enum FieldType
        {
            Text,
            Replacement
        }

        private struct Field
        {
            public FieldType Type;
            public string Str;
        }
    }
}
