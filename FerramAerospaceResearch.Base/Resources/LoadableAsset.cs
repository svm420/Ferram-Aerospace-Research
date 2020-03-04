using System;
using FerramAerospaceResearch.Config;
using FerramAerospaceResearch.Resources.Loading;
using FerramAerospaceResearch.Threading;

namespace FerramAerospaceResearch.Resources
{
    public class LoadableAsset<T> : AssetRequest<T>
    {
        public Loaders<T> AssetLoaders { get; set; }
        public IAssetLoader<T> Loader { get; private set; }

        public string Key { get; set; }

        public event Action<string, AssetRequest<T>> OnAssetLoaded;
        public event Action<string, AssetRequest<T>> OnAssetLoadError;

        private ResourceNode node;

        public ResourceNode Node
        {
            get { return node; }
            set
            {
                Unsubscribe();
                node = value;
                Subscribe();

                Url = node.Url;
                Loader = AssetLoaders.Get(node.Loader);
            }
        }

        private void OnUrlChanged(string url)
        {
            FARLogger.TraceFormat("Asset '{0}' url has changed: '{1}' -> {2}", Key, Url, url);
            Url = url;
            Load();
        }

        private void OnLoaderChanged(string name)
        {
            FARLogger.TraceFormat("Asset '{0}' loader has changed: '{1}' -> {2}", Key, Loader.Name, name);
            Loader = AssetLoaders.Get(name);
            Load();
        }

        public void Load()
        {
            if (Loader is null)
            {
                FARLogger.WarningFormat("Could not load resource from {0} because loader is not set", Url);
                return;
            }

            MainThread.StartCoroutine(() => Loader.Load(this));
        }

        private void Subscribe()
        {
            if (node is null)
                return;

            node.Url.OnValueChanged += OnUrlChanged;
            node.Loader.OnValueChanged += OnLoaderChanged;
        }

        public void Unsubscribe()
        {
            if (node is null)
                return;

            node.Url.OnValueChanged -= OnUrlChanged;
            node.Loader.OnValueChanged -= OnLoaderChanged;
        }

        /// <inheritdoc />
        protected override void AssetLoaded()
        {
            OnAssetLoaded?.Invoke(Key, this);
        }

        /// <inheritdoc />
        protected override void AssetError()
        {
            OnAssetLoadError?.Invoke(Key, this);
        }
    }
}
