using System.Text;
using FerramAerospaceResearch.FARUtils;

namespace FerramAerospaceResearch
{
    public class StringFormatterConfigValue : StringFormatter, IConfigValue<string>
    {
        public StringFormatterConfigValue(string name, string formatString) : base(formatString)
        {
            Name = name;
            Default = formatString;
        }

        public void Reset()
        {
            FormatString = Default;
        }

        public void DebugString(StringBuilder sb)
        {
            FARConfigParser.AppendEntry(sb, Name, FormatString);
        }

        public void Parse(IConfigNode node)
        {
            if (node.TryGetValue(Name, ref formatString))
                Parse(formatString);
        }

        public void Save(IConfigNode node)
        {
            node.AddValue(Name, formatString);
        }

        public string Name { get; }

        public string EditableName
        {
            get { return $"%{Name}"; }
        }

        public string Default { get; }

        public string Value
        {
            get { return FormatString; }
            set { FormatString = value; }
        }
    }
}
