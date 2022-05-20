using System.Collections;
using FerramAerospaceResearch.Interfaces;
using FerramAerospaceResearch.Resources.Loading;
using FerramAerospaceResearch.Threading;
using UnityEngine;

namespace FerramAerospaceResearch.Resources
{
    [FARAddon(0, true)]
    public class FARAssets : MonoSingleton<FARAssets>, IReloadable
    {
        private bool loaded;
        public FARShaderCache Shaders { get; private set; }
        public FARComputeShaderCache ComputeShaders { get; private set; }
        public FARTextureCache Textures { get; private set; }
        public LoaderCache Loaders { get; private set; }

        public ShaderBundleLoader ShaderLoader { get; private set; }
        public ComputeShaderBundleLoader ComputeShaderLoader { get; private set; }

        public int Priority { get; set; }
        public bool Completed { get; set; }

        public void DoReload()
        {
            if (!loaded)
                StartCoroutine(LoadAssets());
            else
                Completed = true;
        }

        protected override void OnAwake()
        {
            // first setup bundle loaders
            ShaderLoader = new ShaderBundleLoader();
            ComputeShaderLoader = new ComputeShaderBundleLoader();

            // then setup loader caches and add loaders
            Loaders = new LoaderCache();
            Loaders.Shaders.Add(new AssetBundleAssetLoader<Shader> {BundleLoader = ShaderLoader});
            Loaders.Textures.Add(new TextureLoader());
            Loaders.Shaders.Add(new ShaderLoader());
            Loaders.ComputeShaders.Add(new ComputeShaderLoader());
            Loaders.ComputeShaders.Add(new AssetBundleAssetLoader<ComputeShader> {BundleLoader = ComputeShaderLoader});

            // finally, setup asset caches
            Shaders = new FARShaderCache();
            Textures = new FARTextureCache();
            ComputeShaders = new FARComputeShaderCache();
        }

        protected override void OnDestruct()
        {
            Textures.Unsubscribe();
            Shaders.Unsubscribe();
            ShaderLoader.Unsubscribe();
            ComputeShaderLoader.Unsubscribe();
        }

        public void ReloadAssets()
        {
            MainThread.StartCoroutine(DoReloadAssets);
        }

        private IEnumerator LoadAssets()
        {
            FARLogger.Debug("Loading all assets");
            if (ShaderLoader.NeedsReload)
                yield return ShaderLoader.Load();

            while (ShaderLoader.State == Progress.InProgress)
                yield return null;

            yield return DoReloadAssets();

            Completed = true;
            loaded = true;
        }

        private IEnumerator DoReloadAssets()
        {
            yield return Shaders.Load();
            yield return ComputeShaders.Load();
            yield return Textures.Load();
        }
    }
}
