using FerramAerospaceResearch.Config;
using UnityEngine;

namespace FerramAerospaceResearch.Resources
{
    public class FARShaderCache : FARAssetDictionary<Shader>
    {
        public FARShaderCache()
        {
            LineRenderer = MakeAsset(FARConfig.Shaders.LineRenderer, "line_renderer");
            DebugVoxel = MakeAsset(FARConfig.Shaders.DebugVoxel, "debug_voxel");
            DebugVoxelFallback = MakeAsset(FARConfig.Shaders.DebugVoxelFallback, "debug_voxel_fallback");
        }

        public ShaderAssetRequest LineRenderer { get; }
        public ShaderAssetRequest DebugVoxel { get; }
        public ShaderAssetRequest DebugVoxelFallback { get; }

        private ShaderAssetRequest MakeAsset(ResourceNode node, string name)
        {
            var asset = new ShaderAssetRequest
            {
                Key = name,
                Node = node,
                AssetLoaders = FARAssets.Instance.Loaders.Shaders
            };
            SetupAsset(asset);
            return asset;
        }

        public class ShaderAssetRequest : LoadableAsset<Shader>
        {
            private ShaderMaterialPair Items { get; } = new ShaderMaterialPair();

            public Material Material
            {
                get { return Items.Material; }
            }

            /// <inheritdoc />
            protected override void AssetLoaded()
            {
                base.AssetLoaded();
                Items.Shader = Asset;
            }


            public static implicit operator ShaderMaterialPair(ShaderAssetRequest rb)
            {
                return rb.Items;
            }

            public static implicit operator Shader(ShaderAssetRequest rb)
            {
                return rb.Items.Shader;
            }

            public static implicit operator Material(ShaderAssetRequest rb)
            {
                return rb.Items.Material;
            }
        }

        public class ShaderMaterialPair
        {
            private Shader shader;

            public Shader Shader
            {
                get { return shader; }
                set
                {
                    shader = value;
                    if (value == null)
                        return;
                    if (Material == null)
                        Material = new Material(value);
                    else
                        Material.shader = value;
                }
            }

            public Material Material { get; private set; }

            public static implicit operator Material(ShaderMaterialPair sm)
            {
                return sm.Material;
            }

            public static implicit operator Shader(ShaderMaterialPair sm)
            {
                return sm.shader;
            }
        }
    }
}
