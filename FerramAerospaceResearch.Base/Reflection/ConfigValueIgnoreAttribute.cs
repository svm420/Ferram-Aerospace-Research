using System;

namespace FerramAerospaceResearch.Reflection
{
    /// <summary>
    /// Attribute to ignore a member in reflection
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConfigValueIgnoreAttribute : Attribute
    {
    }
}
