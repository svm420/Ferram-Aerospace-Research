using FerramAerospaceResearch.Reflection;

namespace FerramAerospaceResearch.Config
{
    [ConfigNode("FlightLog")]
    public class FlightLogConfig
    {
        private string directory = PathUtil.Combine(PathUtil.PParentDir, "Logs", PathUtil.ModDirectoryName);

        [ConfigValue("directory")]
        public string Directory
        {
            get { return directory; }
            set { directory = PathUtil.Combine(PathUtil.PParentDir, value); }
        }

        [ConfigValue("nameFormat")]
        public StringFormatter NameFormat { get; } = new StringFormatter("<<<VESSEL_NAME>>>_<<<DATETIME>>>.csv");

        [ConfigValue("datetimeFormat")]
        public string DatetimeFormat { get; set; } = "yyyy_MM_dd_HH_mm_ss";

        [ConfigValue("period")]
        public Observable<int> LogPeriod { get; } = new Observable<int>(50);

        [ConfigValue("flushPeriod")]
        public Observable<int> FlushPeriod { get; } = new Observable<int>(10);
    }
}
