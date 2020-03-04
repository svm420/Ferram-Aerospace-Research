using System.Threading;
using UnityEngine;

// ReSharper disable StaticMemberInGenericType -

namespace FerramAerospaceResearch
{
    /// <summary>
    ///     Inherit from this base class to create a singleton.
    ///     e.g. public class MyClassName : MonoSingleton&lt;MyClassName&gt; {}
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Check to see if we're about to be destroyed.
        private static bool shuttingDown;

        private static readonly ReaderWriterLockSlim rwLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static volatile T instance;
        private bool destroyingDuplicate;

        /// <summary>
        ///     Access singleton instance through this propriety.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (shuttingDown)
                {
                    FARLogger.Warning($"[MonoSingleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                    return null;
                }

                if (!rwLock.TryEnterUpgradeableReadLock(50))
                {
                    // already entered so the timeout should be enough to have the instance setup
                    return instance;
                }

                try
                {
                    if (instance != null)
                        return instance;

                    rwLock.EnterWriteLock();
                    try
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject($"{typeof(T).ToString()} (MonoSingleton)");
                        singletonObject.AddComponent<T>();

                        // Make instance persistent.
                        DontDestroyOnLoad(singletonObject);

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

        private void Awake()
        {
            if (instance == null)
            {
                FARLogger.Debug($"MonoSingleton {this} is awake");
                instance = this as T;
                OnAwake();
            }

            else
            {
                FARLogger.Trace($"{this} is a MonoSingleton but an instance already exists, destroying self");
                destroyingDuplicate = true;
                Destroy(this);
            }
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnApplicationQuit()
        {
            shuttingDown = true;
        }

        protected void OnDestroy()
        {
            if (!destroyingDuplicate)
            {
                shuttingDown = true;
                OnDestruct();
                instance = null;
            }

            destroyingDuplicate = false;
        }

        protected virtual void OnDestruct()
        {
        }
    }
}
