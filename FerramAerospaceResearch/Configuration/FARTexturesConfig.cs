using System.Text;

namespace FerramAerospaceResearch
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [ConfigParser("textures")]
    public class FARTexturesConfig : FARConfigParser<FARTexturesConfig>
    {
        private string iconButtonBlizzy;
        private string iconButtonStock;
        private string spriteDebugVoxel;

        public string IconButtonBlizzy
        {
            get { return iconButtonBlizzy; }
        }

        public string IconButtonStock
        {
            get { return iconButtonStock; }
        }

        public string SpriteDebugVoxel
        {
            get { return spriteDebugVoxel; }
        }

        public override void Reset()
        {
            base.Reset();
            iconButtonBlizzy = Defaults.IconButtonBlizzy.Value;
            iconButtonStock = Defaults.IconButtonStock.Value;
            spriteDebugVoxel = Defaults.SpriteDebugVoxel.Value;
        }

        public override void Parse(IConfigNode node)
        {
            node.TryGetValue(Defaults.IconButtonBlizzy.Name, ref iconButtonBlizzy);
            node.TryGetValue(Defaults.IconButtonStock.Name, ref iconButtonStock);
            node.TryGetValue(Defaults.SpriteDebugVoxel.Name, ref spriteDebugVoxel);
        }

        public override void SaveTo(IConfigNode node)
        {
            base.SaveTo(node);
            node.AddValue(Defaults.IconButtonBlizzy.EditableName, iconButtonBlizzy);
            node.AddValue(Defaults.IconButtonStock.EditableName, iconButtonStock);
            node.AddValue(Defaults.SpriteDebugVoxel.EditableName, spriteDebugVoxel);
        }

        public override void DebugString(StringBuilder sb)
        {
            base.DebugString(sb);
            AppendEntry(sb, Defaults.IconButtonBlizzy.Name, iconButtonBlizzy);
            AppendEntry(sb, Defaults.IconButtonBlizzy.Name, iconButtonBlizzy);
            AppendEntry(sb, Defaults.IconButtonBlizzy.Name, iconButtonBlizzy);
        }

        public static class Defaults
        {
            public static readonly ConfigValue<string> IconButtonBlizzy =
                new ConfigValue<string>("iconButtonBlizzy", "FerramAerospaceResearch/Textures/icon_button_blizzy");

            public static readonly ConfigValue<string> IconButtonStock =
                new ConfigValue<string>("iconButtonStock", "FerramAerospaceResearch/Textures/icon_button_stock");

            public static readonly ConfigValue<string> SpriteDebugVoxel =
                new ConfigValue<string>("spriteDebugVoxel", "FerramAerospaceResearch/Textures/sprite_debug_voxel");
        }
    }
}
