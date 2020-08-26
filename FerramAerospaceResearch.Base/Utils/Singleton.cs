using System;
using System.Threading;

namespace FerramAerospaceResearch
{
    /// <summary>
    ///     Inherit from this base class to create a singleton.
    ///     e.g. public class MyClassName : Singleton&lt;MyClassName&gt; {}
    /// </summary>
    public class Singleton<T> where T : class
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object locker = new object();
        private static volatile T instance;

        protected Singleton()
        {
            if (instance == null)
            {
                FARLogger.DebugFormat("Singleton {0} is created", this);
            }
            else
            {
                FARLogger.TraceFormat("{0} is a Singleton but an instance already exists", this);
            }
        }

        /// <summary>
        ///     Access singleton instance through this property.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                lock (locker)
                {
                    return instance ??= Activator.CreateInstance(typeof(T), true) as T;
                }
            }
        }
    }
}
