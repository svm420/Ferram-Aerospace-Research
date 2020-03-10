using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FerramAerospaceResearch.FARUtils;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FerramAerospaceResearch
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class FARAssets : MonoBehaviour
    {
        private const string AssetBundleExtension = ".far";

        private static readonly string assetBundleRootPath =
            Path.Combine(Assembly.GetExecutingAssembly().Location, "../../Assets");

        private static readonly string texturesRootPath =
            Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().Location, "../../Textures"));

        public static FARShaderCache ShaderCache { get; private set; }
        public static FARTextureCache TextureCache { get; private set; }

        private void Start()
        {
            ShaderCache = new FARShaderCache("farshaders");
            TextureCache = new FARTextureCache();
            StartCoroutine(LoadAssetsAsync());
        }

        private static IEnumerator LoadAssetsAsync()
        {
            // using a separate method to chain asset loading in the future
            yield return TextureCache.Initialize();
            yield return ShaderCache.LoadAsync();
        }

        public class FARAssetDictionary<T> : Dictionary<string, T> where T : Object
        {
            private AssetBundle assetBundle;
            private string bundleName;

            public FARAssetDictionary(string bundleName)
            {
                BundleName = bundleName;
            }

            public string BundleName
            {
                // ReSharper disable once UnusedMember.Global
                get { return bundleName; }
                private set
                {
                    bundleName = value;
                    SetBundlePath(value);
                }
            }

            public string BundlePath { get; private set; }
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

            protected virtual void SetBundlePath(string name)
            {
                BundlePath = Path.GetFullPath(Path.Combine(assetBundleRootPath, name) + AssetBundleExtension);
            }
        }

        public class FARShaderCache : FARAssetDictionary<Shader>
        {
            public FARShaderCache(string bundleName) : base(bundleName)
            {
            }

            public ShaderMaterialPair LineRenderer { get; private set; }
            public ShaderMaterialPair DebugVoxels { get; private set; }

            protected override void OnLoad()
            {
                LineRenderer = new ShaderMaterialPair(Shader.Find("Hidden/Internal-Colored"));
                if (TryGetValue("FerramAerospaceResearch/Debug Voxel Mesh", out Shader voxelShader))
                {
                    DebugVoxels = new ShaderMaterialPair(voxelShader);
                    DebugVoxels.Material.SetFloat(ShaderPropertyIds.Cutoff, 0.45f);
                }
                else
                {
                    FARLogger.Warning("Could not find voxel mesh shader. Using Sprites/Default for rendering, you WILL see depth artifacts");
                    DebugVoxels = new ShaderMaterialPair(Shader.Find("Sprites/Default"));
                }
            }

            protected override void SetBundlePath(string name)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (Application.platform)
                {
                    case RuntimePlatform.LinuxPlayer:
                    case RuntimePlatform.WindowsPlayer
                        when SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL", StringComparison.Ordinal):
                        FARLogger.Info("Loading shaders from Linux bundle");
                        name += "_linux"; //For OpenGL users on Windows we load the Linux shaders to fix OpenGL issues
                        break;
                    case RuntimePlatform.WindowsPlayer:
                        FARLogger.Info("Loading shaders from Windows bundle");
                        name += "_windows";
                        break;
                    case RuntimePlatform.OSXPlayer:
                        FARLogger.Info("Loading shaders from MacOSX bundle");
                        name += "_macosx";
                        break;
                    default:
                        // Should never reach this
                        FARLogger.Error($"Invalid runtime platform {Application.platform}");
                        break;
                }

                base.SetBundlePath(name);
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

            public IEnumerator Initialize()
            {
                yield return LoadTexture("icon_button_stock.png", texture => IconLarge = texture);
                yield return LoadTexture("icon_button_blizzy.png", texture => IconSmall = texture);
                yield return LoadTexture("sprite_debug_voxel.png", texture => VoxelTexture = texture);
            }

            private IEnumerator LoadTexture(string url, Action<Texture2D> onLoad)
            {
                string path = $@"file://{Path.Combine(texturesRootPath, url)}";
                FARLogger.DebugFormat("Loading texture from {0}", path);
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(path))
                {
                    yield return request.Send();

                    if (!string.IsNullOrEmpty(request.error))
                    {
                        FARLogger.Error($"Error loading texture from {request.url}: {request.error}");
                    }
                    else
                    {
                        FARLogger.DebugFormat("Texture loaded from from {0}", request.url);
                        Texture2D content = DownloadHandlerTexture.GetContent(request);
                        string name = Path.GetFileNameWithoutExtension(url);
                        Add(name, content);
                        onLoad?.Invoke(content);
                    }
                }
            }
        }
    }
}
