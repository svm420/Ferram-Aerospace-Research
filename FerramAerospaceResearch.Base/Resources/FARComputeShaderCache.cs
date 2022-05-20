using FerramAerospaceResearch.Config;
using Unity.Mathematics;
using UnityEngine;

namespace FerramAerospaceResearch.Resources
{
    public readonly struct Kernel
    {
        public readonly string Name;
        public readonly int Index;
        public readonly uint3 ThreadGroupSizes;

        public bool IsValid
        {
            get { return Index >= 0 && !string.IsNullOrEmpty(Name); }
        }

        public Kernel(ComputeShader shader, string name)
        {
            Name = name;
            Index = shader.FindKernel(name);
            if (Index < 0)
                // kernel not found
                ThreadGroupSizes = default;
            else
                shader.GetKernelThreadGroupSizes(Index,
                                                 out ThreadGroupSizes.x,
                                                 out ThreadGroupSizes.y,
                                                 out ThreadGroupSizes.z);
        }
    }

    public class FARComputeShaderCache : FARAssetDictionary<ComputeShader>
    {
        public FARComputeShaderCache()
        {
        }

        private ComputeShaderAssetRequest MakeAsset(ComputeShaderNode node, string name)
        {
            var asset = new ComputeShaderAssetRequest
            {
                AssetLoaders = FARAssets.Instance.Loaders.ComputeShaders,
                Key = name,
                Node = node
            };
            SetupAsset(asset);
            return asset;
        }

        public class ComputeShaderAssetRequest : LoadableAsset<ComputeShader>
        {
            public Kernel InitializeKernel { get; private set; }

            public Kernel MainKernel { get; private set; }

            /// <inheritdoc />
            protected override void AssetLoaded()
            {
                base.AssetLoaded();
                if (Node is not ComputeShaderNode node)
                    return;
                InitializeKernel = new Kernel(Asset, node.InitializeKernel);
                MainKernel = new Kernel(Asset, node.MainKernel);
            }
        }
    }
}
