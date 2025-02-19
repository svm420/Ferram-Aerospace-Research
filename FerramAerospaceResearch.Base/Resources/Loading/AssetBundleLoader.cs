using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FerramAerospaceResearch.Resources.Loading
{
    internal static class AssetBundleCache
    {
        public static readonly Dictionary<string, AssetBundle> LoadedBundles = new();
    }

    public class AssetBundleLoader<T> : IAssetBundleLoader<T> where T : Object
    {
        private string url;

        public AssetBundleLoader(string path = "")
        {
            url = path;
        }

        public Dictionary<string, T> LoadedAssets { get; } = new();

        public bool TryGetAsset(string name, out T asset)
        {
            return LoadedAssets.TryGetValue(name, out asset);
        }

        public string Url
        {
            get { return url; }
            set
            {
                if (url == value)
                    return;
                NeedsReload = true;
                url = value;
            }
        }

        public Progress State { get; set; }

        public IEnumerator Load()
        {
            // wait for the other loading to be done
            if (State == Progress.InProgress)
            {
                yield return new WaitWhile(() => State == Progress.InProgress);
                yield break;
            }

            State = Progress.InProgress;

            // wait for config to be loaded fully
            while (FARConfig.IsLoading)
                yield return null;

            NeedsReload = false;

            string path = Url;
            FARLogger.DebugFormat("Loading asset bundle from {0}", path);
            if (!AssetBundleCache.LoadedBundles.TryGetValue(path, out AssetBundle assetBundle))
            {
                AssetBundleCache.LoadedBundles[path] = null; // make sure only one is loaded
                AssetBundleCreateRequest createRequest = AssetBundle.LoadFromFileAsync(path);
                yield return createRequest;

                assetBundle = createRequest.assetBundle;
                if (assetBundle == null)
                {
                    AssetBundleCache.LoadedBundles.Remove(path);
                    FARLogger.Error($"Could not load asset bundle from {path}");
                    State = Progress.Error;
                    yield break;
                }

                AssetBundleCache.LoadedBundles[path] = assetBundle;
            } else if (assetBundle is null)
            {
                // currently loading this bundle
                while (true)
                {
                    if (!AssetBundleCache.LoadedBundles.TryGetValue(path, out assetBundle))
                    {
                        // failed to load
                        State = Progress.Error;
                        yield break;
                    }

                    if (assetBundle is not null)
                    {
                        // loaded
                        break;
                    }

                    // still loading
                    yield return null;
                }
            }

            AssetBundleRequest loadRequest = assetBundle.LoadAllAssetsAsync<T>();
            yield return loadRequest;

            LoadedAssets.Clear();
            foreach (Object asset in loadRequest.allAssets)
            {
                if (!LoadedAssets.ContainsKey(asset.name))
                    if (asset is T t)
                        LoadedAssets.Add(asset.name, t);
                    else
                        FARLogger
                            .Warning($"Invalid asset type {asset.GetType().ToString()}, expected {typeof(T).ToString()}");
                else
                    FARLogger.DebugFormat("Asset {0} is duplicated", asset);
            }

            FARLogger.DebugFormat("Completed loading assets from {0}", path);

            State = Progress.Completed;
        }

        public bool NeedsReload { get; protected set; } = true;
    }
}
