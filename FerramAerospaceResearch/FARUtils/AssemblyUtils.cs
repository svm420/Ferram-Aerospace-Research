using System;
using System.Collections.Generic;
using System.Reflection;

namespace FerramAerospaceResearch.FARUtils
{
    public static class AssemblyUtils
    {
        public static IEnumerable<Type> FindTypesInAssembly<T>(Assembly assembly, bool inherit = true)
        {
            foreach (Type type in assembly.GetTypes())
                if (type.IsDefined(typeof(T), inherit))
                    yield return type;
        }

        public static IEnumerable<Type> FindTypesInAssembly<T>(bool inherit = true)
        {
            return FindTypesInAssembly<T>(Assembly.GetExecutingAssembly(), inherit);
        }

        public static IEnumerable<Type> FindTypes<T>(
            IEnumerable<AssemblyLoader.LoadedAssembly> loadedAssemblies,
            bool inherit
        )
        {
            // AssemblyLoader.loadedTypes don't have all the types from all the assemblies
            foreach (AssemblyLoader.LoadedAssembly loadedAssembly in loadedAssemblies)
                foreach (Type type in loadedAssembly.assembly.GetTypes())
                    if (type.IsDefined(typeof(T), inherit))
                        yield return type;
        }

        public static IEnumerable<Type> FindTypes<T>(bool inherit)
        {
            return FindTypes<T>(AssemblyLoader.loadedAssemblies, inherit);
        }

        public static IEnumerable<KeyValuePair<T, Type>> FindAttributeInAssembly<T>(
            Assembly assembly,
            bool inherit = true
        ) where T : Attribute
        {
            foreach (Type type in assembly.GetTypes())
                if (type.GetCustomAttribute(typeof(T), inherit) is T attribute)
                    yield return new KeyValuePair<T, Type>(attribute, type);
        }

        public static IEnumerable<KeyValuePair<T, Type>> FindAttributeInAssembly<T>(bool inherit = true)
            where T : Attribute
        {
            return FindAttributeInAssembly<T>(Assembly.GetExecutingAssembly(), inherit);
        }

        public static IEnumerable<KeyValuePair<T, Type>> FindAttribute<T>(
            IEnumerable<AssemblyLoader.LoadedAssembly> loadedAssemblies,
            bool inherit = true
        ) where T : Attribute
        {
            // AssemblyLoader.loadedTypes don't have all the types from all the assemblies
            foreach (AssemblyLoader.LoadedAssembly loadedAssembly in loadedAssemblies)
                foreach (Type type in loadedAssembly.assembly.GetTypes())
                    if (type.GetCustomAttribute(typeof(T), inherit) is T attribute)
                        yield return new KeyValuePair<T, Type>(attribute, type);
        }

        public static IEnumerable<KeyValuePair<T, Type>> FindAttribute<T>(bool inherit = true) where T : Attribute
        {
            return FindAttribute<T>(AssemblyLoader.loadedAssemblies, inherit);
        }
    }
}
