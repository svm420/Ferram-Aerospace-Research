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
    internal class FARAssets : MonoBehaviour
    {
        public enum LoadState
        {
            None,
            InProgress,
            Completed,
            Error
        }

        private static FARAssets instance;
        private static readonly object locker = new object();

        public static FARAssets Instance
        {
            get
            {
                lock (locker)
                {
                    if (instance != null)
                        return instance;

                    Setup();
                    return instance;
                }
            }
        }

        public LoadState State { get; private set; } = LoadState.None;
        public FARShaderCache ShaderCache { get; private set; }
        public FARTextureCache TextureCache { get; private set; }

        private static void Setup()
        {
            var go = new GameObject("FARAssets");
            instance = go.AddComponent<FARAssets>();
            DontDestroyOnLoad(go);
            instance.LoadAssets();
        }

        private void Awake()
        {
            ShaderCache = new FARShaderCache();
            TextureCache = new FARTextureCache();
        }

        private void LoadAssets()
        {
            if (State != LoadState.None)
                return;
            ReloadAssets();
        }

        public static void ModuleManagerPostLoad()
        {
            lock (locker)
            {
                if (instance == null)
                    Setup();
                else
                    instance.ReloadAssets();
            }
        }

        internal void ReloadAssets()
        {
            StartLoading(LoadAssetsAsync());
        }

        private IEnumerator LoadAssetsAsync()
        {
            TextureCache.LoadAll();

            // using a separate method to chain asset loading in the future
            yield return LoadAll();
        }

        internal void ReloadShaders()
        {
            StartLoading(LoadShaders());
        }

        private void StartLoading(IEnumerator enumerator)
        {
            StartCoroutine(WrapLoading(enumerator));
        }

        internal void ReloadTextures()
        {
            TextureCache.LoadAll();
        }

        private IEnumerator WrapLoading(IEnumerator enumerator)
        {
            State = LoadState.InProgress;

            // wait for config to be loaded
            while (FARConfig.State != FARConfig.ConfigState.Loaded)
                yield return null;

            yield return enumerator;
            if (State == LoadState.Error)
            {
                FARLogger.Error("There were errors loading FARAssets");
            }
            else
            {
                State = LoadState.Completed;
            }
        }

        private IEnumerator LoadAll()
        {
            yield return LoadShaders();
        }

        private IEnumerator LoadShaders()
        {
            yield return ShaderCache.LoadAsync();

            if (ShaderCache.State != LoadState.Error)
                yield break;
            FARLogger.Error("There were errors loading FAR shaders.");
            State = LoadState.Error;
        }

        private void OnDestroy()
        {
            TextureCache.Unregister();
            ShaderCache.Unregister();
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

            public LoadState State { get; protected set; } = LoadState.None;

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
                State = LoadState.InProgress;
                AssetsLoaded = false;
                FARLogger.Debug($"Loading asset bundle {BundlePath}");
                AssetBundleCreateRequest createRequest = AssetBundle.LoadFromFileAsync(BundlePath);
                yield return createRequest;

                assetBundle = createRequest.assetBundle;
                if (assetBundle == null)
                {
                    FARLogger.Error($"Could not load asset bundle from {BundlePath}");
                    State = LoadState.Error;
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
                State = LoadState.Completed;
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
                        FARShadersConfig.Instance.BundleLinux.onValueChanged += ReloadAssets;
                        break;
                    case RuntimePlatform.WindowsPlayer:
                        FARLogger.Info("Loading shaders from Windows bundle");
                        BundlePath = FARShadersConfig.Instance.BundleWindows;
                        FARShadersConfig.Instance.BundleWindows.onValueChanged += ReloadAssets;
                        break;
                    case RuntimePlatform.OSXPlayer:
                        FARLogger.Info("Loading shaders from MacOSX bundle");
                        BundlePath = FARShadersConfig.Instance.BundleMac;
                        FARShadersConfig.Instance.BundleMac.onValueChanged += ReloadAssets;
                        break;
                    default:
                        // Should never reach this
                        FARLogger.Error($"Invalid runtime platform {Application.platform}");
                        break;
                }
            }

            public ShaderMaterialPair LineRenderer { get; private set; }
            public ShaderMaterialPair DebugVoxels { get; private set; }

            internal void Unregister()
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (Application.platform)
                {
                    case RuntimePlatform.LinuxPlayer:
                    case RuntimePlatform.WindowsPlayer
                        when SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL", StringComparison.Ordinal):
                        FARShadersConfig.Instance.BundleLinux.onValueChanged -= ReloadAssets;
                        break;
                    case RuntimePlatform.WindowsPlayer:
                        FARShadersConfig.Instance.BundleWindows.onValueChanged -= ReloadAssets;
                        break;
                    case RuntimePlatform.OSXPlayer:
                        FARShadersConfig.Instance.BundleMac.onValueChanged -= ReloadAssets;
                        break;
                    default:
                        // Should never reach this
                        FARLogger.Error($"Invalid runtime platform {Application.platform}");
                        break;
                }
            }

            private void ReloadAssets(IConfigValue<string> bundlePath)
            {
                BundlePath = bundlePath.Value;
                Instance.ReloadShaders();
            }

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
            public FARTextureCache()
            {
                Add("icon_button_stock", IconLarge);
                Add("icon_button_blizzy", IconSmall);
                Add("sprite_debug_voxel", VoxelTexture);

                FARTexturesConfig config = FARTexturesConfig.Instance;
                config.IconButtonStock.onValueChanged += ReloadStockButton;
                config.IconButtonBlizzy.onValueChanged += ReloadBlizzyButton;
                config.SpriteDebugVoxel.onValueChanged += ReloadVoxel;

                GameEvents.OnGameDatabaseLoaded.Add(LoadAll);
            }

            public bool Loaded { get; private set; }
            public Texture2D IconLarge { get; private set; }
            public Texture2D IconSmall { get; private set; }
            public Texture2D VoxelTexture { get; private set; }

            internal void Unregister()
            {
                FARTexturesConfig config = FARTexturesConfig.Instance;
                config.IconButtonStock.onValueChanged -= ReloadStockButton;
                config.IconButtonBlizzy.onValueChanged -= ReloadBlizzyButton;
                config.SpriteDebugVoxel.onValueChanged -= ReloadVoxel;
                GameEvents.OnGameDatabaseLoaded.Remove(LoadAll);
            }

            public void LoadAll()
            {
                if (!GameDatabase.Instance.IsReady())
                {
                    FARLogger.Debug("Trying to load textures before GameDatabase has been loaded");
                    return;
                }

                FARTexturesConfig config = FARTexturesConfig.Instance;
                ReloadStockButton(config.IconButtonStock);
                ReloadBlizzyButton(config.IconButtonBlizzy);
                ReloadVoxel(config.SpriteDebugVoxel);

                Loaded = true;
            }

            private void ReloadStockButton(IConfigValue<string> value)
            {
                IconLarge = GameDatabase.Instance.GetTexture(value.Value, false);
                this["icon_button_stock"] = IconLarge;
            }

            private void ReloadBlizzyButton(IConfigValue<string> value)
            {
                IconSmall = GameDatabase.Instance.GetTexture(value.Value, false);
                this["icon_button_blizzy"] = IconSmall;
            }

            private void ReloadVoxel(IConfigValue<string> value)
            {
                VoxelTexture = GameDatabase.Instance.GetTexture(value.Value, false);
                this["sprite_debug_voxel"] = VoxelTexture;
            }
        }
    }
}
