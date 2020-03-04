using FerramAerospaceResearch.Reflection;

namespace FerramAerospaceResearch.Config
{
    [ConfigNode("Debug")]
    public class DebugConfig
    {
        [ConfigValue("logLevel")]
        public static LogLevel Level
        {
            get { return FARLogger.Level; }
            set { FARLogger.Level = value; }
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global - reflection
        [ConfigValue("dumpOnLoad")]
        public bool DumpOnLoad { get; set; } = false;
    }
}
