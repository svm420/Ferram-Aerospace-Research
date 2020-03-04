namespace FerramAerospaceResearch.Interfaces
{
    /// <summary>
    /// Interface for config nodes that need to do setup before/after loading/saving them
    /// </summary>
    public interface IConfigNode
    {
        /// <summary>
        /// Called before loading this config node
        /// </summary>
        void BeforeLoaded();

        /// <summary>
        /// Called after loading this config node
        /// </summary>
        void AfterLoaded();

        /// <summary>
        /// Called before saving this config node
        /// </summary>
        void BeforeSaved();

        /// <summary>
        /// Called after saving this config node
        /// </summary>
        void AfterSaved();
    }
}
