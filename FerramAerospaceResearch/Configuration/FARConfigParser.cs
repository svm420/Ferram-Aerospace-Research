using System.Reflection;
using System.Text;

namespace FerramAerospaceResearch
{
    public abstract class FARConfigParser
    {
        internal static string GetConfigName<T>() where T : FARConfigParser
        {
            var attribute = typeof(T).GetCustomAttribute(typeof(ConfigParserAttribute), true) as ConfigParserAttribute;
            return attribute?.Name;
        }

        public virtual void Reset()
        {
        }

        public abstract void Parse(IConfigNode node);

        public virtual void SaveTo(IConfigNode node)
        {
        }

        public virtual void DebugString(StringBuilder sb)
        {
        }

        protected static void AppendEntry<T>(StringBuilder sb, string name, T value)
        {
            sb.Append("    ").Append(name).Append(": ").AppendLine(value.ToString());
        }
    }

    public abstract class FARConfigParser<T> : FARConfigParser where T : FARConfigParser
    {
        // ReSharper disable once StaticMemberInGenericType - that's the idea
        private static string configName;

        public static string ConfigName
        {
            get { return configName ??= GetConfigName<T>(); }
        }

        public static T Instance
        {
            get { return FARConfig.Instance[ConfigName] as T; }
        }
    }
}
