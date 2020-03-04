// ReSharper disable UnusedMemberInSuper.Global

namespace FerramAerospaceResearch.Interfaces
{
    /// <summary>
    /// Interface for config values that are not supported types
    /// </summary>
    public interface IConfigValue
    {
        void Set(object value);
        object Get();
    }

    /// <summary>
    /// Generic version of IConfigValue
    /// </summary>
    /// <typeparam name="T">type of the config value</typeparam>
    public interface IConfigValue<T> : IConfigValue
    {
        void Set(T value);
        new T Get();
    }
}
