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
    }
}
