using System.Collections.Generic;
using FerramAerospaceResearch.Resources.Loading;
using UnityEngine;

namespace FerramAerospaceResearch.Resources
{
    public class LoaderCache
    {
        public Loaders<Shader> Shaders { get; } = new();
        public Loaders<ComputeShader> ComputeShaders { get; } = new();
        public Loaders<Texture2D> Textures { get; } = new();
    }

    public class Loaders<T> : Dictionary<string, IAssetLoader<T>>
    {
        private IAssetLoader<T> defaultLoader;

        public Loaders()
        {
            Add("default", null);
        }

        public IAssetLoader<T> Default
        {
            get { return defaultLoader; }
            set
            {
                defaultLoader = value;
                this["default"] = value;
            }
        }

        public IAssetLoader<T> Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Default;
            return TryGetValue(name, out IAssetLoader<T> loader) ? loader : Default;
        }

        public void Add(IAssetLoader<T> loader)
        {
            if (string.IsNullOrEmpty(loader.Name) || loader.Name == "default")
                Default = loader;
            else
                base.Add(loader.Name, loader);
        }
    }
}
