using System.Collections;

namespace FerramAerospaceResearch.Reflection
{
    public interface INodeLoader
    {
        /// <summary>
        /// Plain value callback
        /// </summary>
        /// <param name="reflection">Value reflection</param>
        /// <param name="newValue">New value to set in the owner object</param>
        /// <returns>Whether the the owner object should be updated with <paramref name="newValue"/></returns>
        bool OnValue(ValueReflection reflection, out object newValue);

        /// <summary>
        /// List plain value callback
        /// </summary>
        /// <param name="reflection">List value reflection associated with currently iterated list</param>
        /// <param name="newValue">New value to set in the owner object</param>
        /// <returns>Whether the the owner object should be updated with <paramref name="newValue"/></returns>
        bool OnValueList(ListValueReflection reflection, out IList newValue);

        /// <summary>
        /// Nested node callback
        /// </summary>
        /// <param name="nodeObject"></param>
        /// <param name="reflection">Node reflection</param>
        /// <param name="newValue">New value to set in the owner object</param>
        /// <returns>Whether the the owner object should be updated with <paramref name="newValue"/></returns>
        bool OnNode(object nodeObject, NodeReflection reflection, out object newValue);

        /// <summary>
        /// List nested node callback
        /// </summary>
        /// <param name="reflection">List reflection</param>
        /// <param name="nodeReflection">Node reflection</param>
        /// <param name="newValue">New value to set in the owner object</param>
        /// <returns>Whether the the owner object should be updated with <paramref name="newValue"/></returns>
        bool OnNodeList(ListValueReflection reflection, NodeReflection nodeReflection, out IList newValue);
    }
}
