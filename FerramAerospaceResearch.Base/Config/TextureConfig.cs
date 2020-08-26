using FerramAerospaceResearch.Reflection;

namespace FerramAerospaceResearch.Config
{
    [ConfigNode("Textures")]
    public class TextureConfig
    {
        [ConfigValue("iconButtonBlizzy")]
        public TextureNode IconButtonBlizzy { get; } =
            new TextureNode("FerramAerospaceResearch/Textures/icon_button_blizzy");

        [ConfigValue("iconButtonStock")]
        public TextureNode IconButtonStock { get; } =
            new TextureNode("FerramAerospaceResearch/Textures/icon_button_stock");

        [ConfigValue("spriteDebugVoxel")]
        public TextureNode SpriteDebugVoxel { get; } =
            new TextureNode("FerramAerospaceResearch/Textures/sprite_debug_voxel");
    }
}
