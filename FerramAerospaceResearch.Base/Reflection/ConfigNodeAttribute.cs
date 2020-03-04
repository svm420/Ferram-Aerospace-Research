using System;

namespace FerramAerospaceResearch.Reflection
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ConfigNodeAttribute : Attribute
    {
        /// <summary>
        ///     Whether there can be multiple nodes of this, should only be used if IsRoot is true
        /// </summary>
        public readonly bool AllowMultiple;

        /// <summary>
        ///     Node name
        /// </summary>
        public readonly string Id;

        /// <summary>
        ///     Whether this node is root in config files
        /// </summary>
        public readonly bool IsRoot;

        public ConfigNodeAttribute(string id, bool isRoot = false, bool allowMultiple = false)
        {
            Id = id;
            IsRoot = isRoot;
            AllowMultiple = allowMultiple;
        }
    }
}
