using System;
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
            string str = string.Empty;
            if (node.TryGetValue(Name, ref str))
                Parse(str);
        }

        public void Save(IConfigNode node)
        {
            node.AddValue(Name, FormatString);
        }

        public event Action<IConfigValue> onChanged;
        public event Action<IConfigValue<string>> onValueChanged;
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

        public override void Parse(string str)
        {
            string old = FormatString;
            base.Parse(str);

            if (old == FormatString)
                return;
            onChanged?.Invoke(this);
            onValueChanged?.Invoke(this);
        }
    }
}
