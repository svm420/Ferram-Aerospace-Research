using System;
// ReSharper disable UnusedMemberInSuper.Global

namespace FerramAerospaceResearch.Interfaces
{
    /// <summary>
    /// Interface for objects that should notify subscribers if the stored value changes
    /// </summary>
    public interface IObservable
    {
        object Value { get; set; }
        // event Action OnValueChanged;
    }

    /// <summary>
    /// Generic version of IObservable
    /// </summary>
    /// <typeparam name="T">type of the stored value</typeparam>
    public interface IObservable<T> : IObservable
    {
        new T Value { get; set; }

        // ReSharper disable once EventNeverSubscribedTo.Global
        /* new */ event Action<T> OnValueChanged;
    }
}
