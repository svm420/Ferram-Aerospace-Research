using System.Collections;
using UnityEngine;

namespace FerramAerospaceResearch.Resources.Loading
{
    public interface IAssetBundleLoader
    {
        string Url { get; set; }
        Progress State { get; }
        bool NeedsReload { get; }
        IEnumerator Load();
    }

    public interface IAssetBundleLoader<T> : IAssetBundleLoader where T : Object
    {
        bool TryGetAsset(string name, out T asset);
    }
}
