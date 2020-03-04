using System;

namespace FerramAerospaceResearch.Reflection
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConfigValueAttribute : Attribute
    {
        /// <summary>
        /// Override name of this config value
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Override type of this config value
        /// </summary>
        public readonly Type Type;

        public ConfigValueAttribute(string name = null, Type type = null)
        {
            Type = type;
            Name = name;
        }
    }
}
