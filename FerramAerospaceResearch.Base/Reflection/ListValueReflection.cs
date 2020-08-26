using System;
using System.Reflection;

namespace FerramAerospaceResearch.Reflection
{
    /// <summary>
    /// A reflection for a list of values
    /// </summary>
    public class ListValueReflection : ValueReflection
    {
        /// <summary>
        /// Id of the config node if this is a reflection of a list of config nodes
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Whether this reflection is a list of config nodes
        /// </summary>
        public bool IsNodeValue
        {
            get { return !string.IsNullOrEmpty(NodeId); }
        }

        protected ListValueReflection()
        {
        }

        public static ListValueReflection Create(
            MemberInfo mi,
            ConfigValueAttribute attribute = null,
            Type valueType = null
        )
        {
            var reflection = new ListValueReflection();
            if (valueType != null)
                reflection.ValueType = valueType;
            return Factory(reflection, mi, attribute);
        }

        protected override void OnSetup(Type type, ConfigValueAttribute attribute)
        {
            ValueType = ReflectionUtils.ListType(ValueType) ?? ValueType;

            if (attribute != null)
                Name = attribute.Name;

            ConfigNodeAttribute node = ValueType.GetCustomAttribute<ConfigNodeAttribute>();
            NodeId ??= node?.Id;
        }
    }
}
