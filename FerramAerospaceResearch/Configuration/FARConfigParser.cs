using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FerramAerospaceResearch.FARUtils;

namespace FerramAerospaceResearch
{
    public abstract class FARConfigParser
    {
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

        public static void AppendEntry<T>(StringBuilder sb, string name, T value)
        {
            sb.Append("    ").Append(name).Append(": ").AppendLine(value.ToString());
        }
    }

    // use CRTP to enable building parsing via reflection
    public abstract class FARConfigParser<T> : FARConfigParser where T : FARConfigParser
    {
        // ReSharper disable once StaticMemberInGenericType
        private static List<PropertyInfo> configValueProperties;

        // ReSharper disable once StaticMemberInGenericType - that's the idea
        private static string configName;

        protected FARConfigParser()
        {
            ConfigValues = GetConfigValues();
        }

        public static string ConfigName
        {
            get { return configName ??= GetConfigName(); }
        }

        public static T Instance
        {
            get { return FARConfig.Instance[ConfigName] as T; }
        }

        private static List<PropertyInfo> ConfigValueProperties
        {
            get { return configValueProperties ??= BuildProperties(); }
        }

        public List<IConfigValue> ConfigValues { get; }

        internal static string GetConfigName()
        {
            var attribute = typeof(T).GetCustomAttribute(typeof(ConfigParserAttribute), true) as ConfigParserAttribute;
            return attribute?.Name ?? string.Empty;
        }

        private List<IConfigValue> GetConfigValues()
        {
            var list = new List<IConfigValue>();
            foreach (PropertyInfo property in ConfigValueProperties)
                list.Add(property.GetValue(this) as IConfigValue);

            return list;
        }

        private static List<PropertyInfo> BuildProperties()
        {
            var values = new List<PropertyInfo>();
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (typeof(IConfigValue).IsAssignableFrom(property.PropertyType))
                    values.Add(property);
            }

            FARLogger.Debug($"{typeof(T).ToString()} found {values.Count.ToString()} config value properties: {string.Join(", ", from property in values select property.Name)}");
            return values;
        }

        public override void Reset()
        {
            base.Reset();
            foreach (IConfigValue value in ConfigValues)
                value.Reset();
        }

        public override void DebugString(StringBuilder sb)
        {
            base.DebugString(sb);
            foreach (IConfigValue value in ConfigValues)
                value.DebugString(sb);
        }

        public override void SaveTo(IConfigNode node)
        {
            base.SaveTo(node);
            foreach (IConfigValue value in ConfigValues)
                value.Save(node);
        }

        public override void Parse(IConfigNode node)
        {
            foreach (IConfigValue value in ConfigValues)
                value.Parse(node);
        }
    }
}
