namespace FerramAerospaceResearch.Reflection
{
    public interface INodeSaver
    {
        /// <summary>
        /// Plain value callback
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="reflection">Value reflection associated with <paramref name="value"/></param>
        /// <param name="newValue">New value to set in the owner object</param>
        /// <returns>Whether the owner object should be updated with <paramref name="newValue"/></returns>
        bool OnValue(object value, ValueReflection reflection, out object newValue);

        /// <summary>
        /// List plain value callback
        /// </summary>
        /// <param name="index">Index of the current value</param>
        /// <param name="value">Current value</param>
        /// <param name="reflection">List value reflection associated with currently iterated list</param>
        /// <param name="newValue">New value to set in the owner object</param>
        /// <returns>Whether the the owner object should be updated with <paramref name="newValue"/> at index <paramref name="index"/></returns>
        bool OnListValue(int index, object value, ListValueReflection reflection, out object newValue);

        /// <summary>
        /// Nested node callback
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="reflection">Node reflection associated with <paramref name="value"/></param>
        /// <param name="newValue">New value to set in the owner object</param>
        /// <returns>Whether the the owner object should be updated with <paramref name="newValue"/></returns>
        bool OnNode(object value, NodeReflection reflection, out object newValue);

        /// <summary>
        /// List nested node callback
        /// </summary>
        /// <param name="index">Index of the current value</param>
        /// <param name="value">Current value</param>
        /// <param name="reflection">List reflection associated with the current member</param>
        /// <param name="nodeReflection">Node reflection associated with <paramref name="value"/></param>
        /// <param name="newValue">New value to set in the owner object</param>
        /// <returns>Whether the the owner object should be updated with <paramref name="newValue"/> at index <paramref name="index"/></returns>
        bool OnListNode(
            int index,
            object value,
            ListValueReflection reflection,
            NodeReflection nodeReflection,
            out object newValue
        );
    }
}
