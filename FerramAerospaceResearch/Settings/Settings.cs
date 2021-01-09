using FerramAerospaceResearch.Reflection;

namespace FerramAerospaceResearch.Settings
{
    [ConfigNode("Settings", parent: typeof(FARConfig))]
    public static class FARSettings
    {
        [ConfigValue("exposedAreaUsesKSPHack")]
        public static bool ExposedAreaUsesKSPHack = true;
        [ConfigValue("exposedAreaLimited")]
        public static bool ExposedAreaLimited = true;

        [ConfigValue("submergedDragMultiplier")] public static double SubmergedDragMultiplier = 0.25;
        [ConfigValue("submergedLiftMultiplier")] public static double SubmergedLiftMultiplier = 0.25;
    }
}
