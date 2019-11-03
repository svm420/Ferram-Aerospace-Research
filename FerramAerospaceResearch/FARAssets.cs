using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FerramAerospaceResearch.FARUtils;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FerramAerospaceResearch
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class FARAssets : MonoBehaviour
    {
        public static FARShaderCache ShaderCache { get; private set; }
        public static FARTextureCache TextureCache { get; private set; }

        private void Start()
        {
            ShaderCache = new FARShaderCache();
            TextureCache = new FARTextureCache();
            TextureCache.Initialize();
            StartCoroutine(LoadAssetsAsync());
        }

        private static IEnumerator LoadAssetsAsync()
        {
            // using a separate method to chain asset loading in the future
            yield return ShaderCache.LoadAsync();
        }

        public class FARAssetDictionary<T> : Dictionary<string, T> where T : Object
        {
            private AssetBundle assetBundle;

            private string bundlePath;

            public FARAssetDictionary()
            {
            }

            public FARAssetDictionary(string bundlePath)
            {
                BundlePath = bundlePath;
            }

            public string BundleName
            {
                get { return Path.GetFileName(BundlePath); }
            }

            public string BundleDirectory
            {
                get { return Path.GetDirectoryName(BundlePath); }
            }

            public string BundlePath
            {
                get { return bundlePath; }
                set { bundlePath = FARConfig.CombineGameData(value); }
            }

            public bool AssetsLoaded { get; private set; }

            public IEnumerator LoadAsync()
            {
                FARLogger.Debug($"Loading asset bundle {BundlePath}");
                AssetBundleCreateRequest createRequest = AssetBundle.LoadFromFileAsync(BundlePath);
                yield return createRequest;

                assetBundle = createRequest.assetBundle;
                if (assetBundle == null)
                {
                    FARLogger.Error($"Could not load asset bundle from {BundlePath}");
                    yield break;
                }

                AssetBundleRequest loadRequest = assetBundle.LoadAllAssetsAsync(typeof(T));
                yield return loadRequest;

                foreach (Object asset in loadRequest.allAssets)
                {
                    FARLogger.Debug($"Adding {asset} to dictionary");
                    Add(asset.name, (T)asset);
                }

                FARLogger.Debug($"Finished loading {typeof(T)} assets from {BundlePath}");
                AssetsLoaded = true;

                OnLoad();
            }

            protected virtual void OnLoad()
            {
            }
        }

        public class FARShaderCache : FARAssetDictionary<Shader>
        {
            public FARShaderCache()
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (Application.platform)
                {
                    case RuntimePlatform.LinuxPlayer:
                    case RuntimePlatform.WindowsPlayer
                        when SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL", StringComparison.Ordinal):
                        FARLogger.Info("Loading shaders from Linux bundle");
                        //For OpenGL users on Windows we load the Linux shaders to fix OpenGL issues
                        BundlePath = FARShadersConfig.Instance.BundleLinux;
                        break;
                    case RuntimePlatform.WindowsPlayer:
                        FARLogger.Info("Loading shaders from Windows bundle");
                        BundlePath = FARShadersConfig.Instance.BundleWindows;
                        break;
                    case RuntimePlatform.OSXPlayer:
                        FARLogger.Info("Loading shaders from MacOSX bundle");
                        BundlePath = FARShadersConfig.Instance.BundleMac;
                        break;
                    default:
                        // Should never reach this
                        FARLogger.Error($"Invalid runtime platform {Application.platform}");
                        break;
                }
            }

            public ShaderMaterialPair LineRenderer { get; private set; }
            public ShaderMaterialPair DebugVoxels { get; private set; }

            protected override void OnLoad()
            {
                LineRenderer = new ShaderMaterialPair(Shader.Find(FARShadersConfig.Instance.LineRenderer));
                if (TryGetValue(FARShadersConfig.Instance.DebugVoxel, out Shader voxelShader))
                {
                    DebugVoxels = new ShaderMaterialPair(voxelShader);
                    DebugVoxels.Material.SetFloat(ShaderPropertyIds.Cutoff, FARShadersConfig.Instance.DebugVoxelCutoff);
                }
                else
                {
                    FARLogger.Warning($"Could not find voxel mesh shader. Using fallback shader {FARShadersConfig.Instance.DebugVoxelFallback}, you WILL likely see depth artifacts");
                    DebugVoxels = new ShaderMaterialPair(Shader.Find(FARShadersConfig.Instance.DebugVoxelFallback));
                }
            }

            public class ShaderMaterialPair
            {
                public ShaderMaterialPair(Shader shader) : this(shader, new Material(shader))
                {
                }

                public ShaderMaterialPair(Shader shader, Material material)
                {
                    Shader = shader;
                    Material = material;
                }

                public Shader Shader { get; }
                public Material Material { get; }
            }
        }

        public class FARTextureCache : Dictionary<string, Texture2D>
        {
            public Texture2D IconLarge { get; private set; }
            public Texture2D IconSmall { get; private set; }
            public Texture2D VoxelTexture { get; private set; }

            public void Initialize()
            {
                Add("icon_button_stock",
                    GameDatabase.Instance.GetTexture(FARTexturesConfig.Instance.IconButtonStock, false));
                Add("icon_button_blizzy",
                    GameDatabase.Instance.GetTexture(FARTexturesConfig.Instance.IconButtonBlizzy, false));
                Add("sprite_debug_voxel",
                    GameDatabase.Instance.GetTexture(FARTexturesConfig.Instance.SpriteDebugVoxel, false));
                IconLarge = this["icon_button_stock"];
                IconSmall = this["icon_button_blizzy"];
                VoxelTexture = this["sprite_debug_voxel"];
            }
        }
    }
}
