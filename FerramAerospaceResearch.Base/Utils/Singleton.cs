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
        private static readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        private static volatile T instance;

        protected Singleton()
        {
            if (instance == null)
            {
                FARLogger.DebugFormat("Singleton {0} is created", this);
                instance = this as T;
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
                rwLock.EnterUpgradeableReadLock();
                try
                {
                    if (instance != null)
                        return instance;

                    rwLock.EnterWriteLock();
                    try
                    {
                        Activator.CreateInstance(typeof(T), true);

                        if (instance == null)
                            FARLogger.Error($"Error instantiating Singleton with type {typeof(T).ToString()}");

                        return instance;
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();
                    }
                }
                finally
                {
                    rwLock.ExitUpgradeableReadLock();
                }
            }
        }
    }
}
