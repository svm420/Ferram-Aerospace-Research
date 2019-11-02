using System;

namespace FerramAerospaceResearch
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigParserAttribute : Attribute
    {
        public string Name { get; }

        public ConfigParserAttribute(string name)
        {
            Name = name;
        }
    }
}
