using FerramAerospaceResearch.Config;
using UnityEngine;

namespace FerramAerospaceResearch.Resources
{
    public class FARTextureCache : FARAssetDictionary<Texture2D>
    {
        public readonly LoadableAsset<Texture2D> DebugVoxel;
        public readonly LoadableAsset<Texture2D> IconLarge;
        public readonly LoadableAsset<Texture2D> IconSmall;

        public FARTextureCache()
        {
            IconLarge = MakeAsset(FARConfig.Textures.IconButtonStock, "stock_button");
            IconSmall = MakeAsset(FARConfig.Textures.IconButtonBlizzy, "blizzy_button");
            DebugVoxel = MakeAsset(FARConfig.Textures.SpriteDebugVoxel, "debug_voxel");
        }

        private LoadableAsset<Texture2D> MakeAsset(ResourceNode node, string name)
        {
            var asset = new LoadableAsset<Texture2D>
            {
                AssetLoaders = FARAssets.Instance.Loaders.Textures,
                Key = name,
                Node = node
            };
            SetupAsset(asset);
            return asset;
        }
    }
}
