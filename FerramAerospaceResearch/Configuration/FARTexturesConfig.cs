namespace FerramAerospaceResearch
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [ConfigParser("textures")]
    public class FARTexturesConfig : FARConfigParser<FARTexturesConfig>
    {
        public StringConfigValue IconButtonBlizzy { get; } =
            new StringConfigValue("iconButtonBlizzy", "FerramAerospaceResearch/Textures/icon_button_blizzy");

        public StringConfigValue IconButtonStock { get; } =
            new StringConfigValue("iconButtonStock", "FerramAerospaceResearch/Textures/icon_button_stock");

        public StringConfigValue SpriteDebugVoxel { get; } =
            new StringConfigValue("spriteDebugVoxel", "FerramAerospaceResearch/Textures/sprite_debug_voxel");
    }
}
