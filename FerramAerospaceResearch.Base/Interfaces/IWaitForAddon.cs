namespace FerramAerospaceResearch.Interfaces
{
    /// <summary>
    /// Interface for addons that should be waited for instantiation
    /// </summary>
    public interface IWaitForAddon
    {
        /// <summary>
        /// Whether this addon has completed its instantiation
        /// </summary>
        bool Completed { get; }
    }
}
