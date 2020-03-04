using FerramAerospaceResearch.Config;
using FerramAerospaceResearch.Reflection;

namespace FerramAerospaceResearch
{
    [ConfigNode("FARConfig", true)]
    public static class FARConfig
    {
        /// <summary>
        /// Whether the config is being loaded
        /// </summary>
        private static volatile bool isLoading;

        public static readonly ShaderConfig Shaders = new ShaderConfig();

        public static readonly TextureConfig Textures = new TextureConfig();

        public static readonly FlightLogConfig FlightLog = new FlightLogConfig();

        public static readonly GUIColors GUIColors = new GUIColors();

        public static readonly DebugConfig Debug = new DebugConfig();

        public static bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; }
        }
    }
}
