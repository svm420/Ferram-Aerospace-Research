using System;

namespace FerramAerospaceResearch
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class FARAddonAttribute : Attribute
    {
        /// <summary>
        /// Whether this addon should have its reference kept after instantiation
        /// </summary>
        public readonly bool Persistant;

        /// <summary>
        /// Priority of instantiation, higher values get instantiated earlier
        /// </summary>
        public readonly int Priority;

        public FARAddonAttribute(int priority = 0, bool persistant = false)
        {
            Priority = priority;
            Persistant = persistant;
        }
    }
}
