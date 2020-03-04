namespace FerramAerospaceResearch.Interfaces
{
    /// <summary>
    /// Interface for objects that can be reloaded such as by ModuleManager
    /// </summary>
    public interface IReloadable
    {
        /// <summary>
        /// Priority of when this object should be reloaded, higher values are reloaded first
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Whether the reload has finished in case DoReload simply starts a coroutine. Is set to false before calling <see cref="DoReload"/>
        /// </summary>
        bool Completed { get; set; }

        /// <summary>
        /// Start or do the reload
        /// </summary>
        void DoReload();
    }
}
