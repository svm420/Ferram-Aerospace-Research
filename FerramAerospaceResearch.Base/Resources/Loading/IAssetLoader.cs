using System.Collections;

namespace FerramAerospaceResearch.Resources.Loading
{
    public interface IAssetLoader
    {
        // using interface property instead of attribute for possibly stateful loaders
        string Name { get; }
    }

    public interface IAssetLoader<out T> : IAssetLoader
    {
        IEnumerator Load(IAssetRequest<T> assetRequest);
    }
}
