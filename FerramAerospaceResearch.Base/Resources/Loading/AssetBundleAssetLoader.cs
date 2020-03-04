using System.Collections;
using UnityEngine;

namespace FerramAerospaceResearch.Resources.Loading
{
    public class AssetBundleAssetLoader<T> : IAssetLoader<T> where T : Object
    {
        public IAssetBundleLoader<T> BundleLoader { get; set; }

        /// <inheritdoc />
        public IEnumerator Load(IAssetRequest<T> assetRequest)
        {
            FARLogger.Assert(assetRequest.Url != null, "Request url cannot be null");
            assetRequest.State = Progress.InProgress;

            // make sure bundle is loaded
            if (BundleLoader.NeedsReload)
                yield return BundleLoader.Load();

            // check for errors
            if (BundleLoader.State == Progress.Error)
            {
                FARLogger.ErrorFormat("Could not load asset bundle {0} for request {1}",
                                      BundleLoader.Url,
                                      assetRequest.Url);
                assetRequest.State = Progress.Error;
                assetRequest.OnError();
                yield break;
            }

            if (!BundleLoader.TryGetAsset(assetRequest.Url, out T asset))
            {
                FARLogger.ErrorFormat("Could not find asset {0} in bundle {1}", assetRequest.Url, BundleLoader.Url);
                assetRequest.State = Progress.Error;
                assetRequest.OnError();
                yield break;
            }

            assetRequest.State = Progress.Completed;
            assetRequest.OnLoad(asset);
        }

        /// <inheritdoc />
        public string Name { get; } = "bundle";
    }
}
