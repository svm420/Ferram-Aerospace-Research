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

        public static readonly ShaderConfig Shaders = new();

        public static readonly TextureConfig Textures = new();

        public static readonly FlightLogConfig FlightLog = new();

        public static readonly GUIColors GUIColors = new();

        public static readonly DebugConfig Debug = new();

        public static readonly VoxelizationConfig Voxelization = new();

        public static readonly ExposureConfig Exposure = new();

        public static bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; }
        }
    }
}
