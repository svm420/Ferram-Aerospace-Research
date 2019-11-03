using System.IO;
using System.Text;
using FerramAerospaceResearch.FARUtils;

namespace FerramAerospaceResearch
{
    // ReSharper disable once ClassNeverInstantiated.Global - instantiated through reflection
    [ConfigParser("flightLog")]
    public class FlightLogConfig : FARConfigParser<FlightLogConfig>
    {
        private string directory;
        private string datetimeFormat;
        private int period;
        private int flushPeriod;

        public string Directory
        {
            get { return directory; }
        }

        public string DatetimeFormat
        {
            get { return datetimeFormat; }
        }

        public int Period
        {
            get { return period; }
        }

        public int FlushPeriod
        {
            get { return flushPeriod; }
        }

        public StringFormatter NameFormat { get; } = new StringFormatter(Defaults.NameFormat.Value);

        public override void Reset()
        {
            directory = Defaults.Directory.Value;
            datetimeFormat = Defaults.DatetimeFormat.Value;
            NameFormat.FormatString = Defaults.NameFormat.Value;
            period = Defaults.Period.Value;
            flushPeriod = Defaults.FlushPeriod.Value;
        }

        public override void Parse(IConfigNode node)
        {
            if (node.TryGetValue(Defaults.Directory.Name, ref directory))
                if (!Path.IsPathRooted(directory))
                    directory = Path.Combine(FARConfig.KSPRootPath, directory);

            node.TryGetValue(Defaults.DatetimeFormat.Name, ref datetimeFormat);

            string str = string.Empty;
            if (node.TryGetValue(Defaults.NameFormat.Name, ref str))
                NameFormat.FormatString = str;

            node.TryGetValue(Defaults.Period.Name, ref period);
            node.TryGetValue(Defaults.FlushPeriod.Name, ref flushPeriod);
        }

        public override void SaveTo(IConfigNode node)
        {
            node.AddValue(Defaults.Directory.EditableName, directory);
            node.AddValue(Defaults.NameFormat.EditableName, NameFormat.FormatString);
            node.AddValue(Defaults.DatetimeFormat.EditableName, datetimeFormat);
            node.AddValue(Defaults.Period.EditableName, period);
            node.AddValue(Defaults.FlushPeriod.EditableName, flushPeriod);
        }

        public override void DebugString(StringBuilder sb)
        {
            AppendEntry(sb, Defaults.Directory.Name, directory);
            AppendEntry(sb, Defaults.NameFormat.Name, NameFormat.FormatString);
            AppendEntry(sb, Defaults.DatetimeFormat.Name, datetimeFormat);
            AppendEntry(sb, Defaults.Period.Name, period);
            AppendEntry(sb, Defaults.FlushPeriod.Name, flushPeriod);
        }

        public static class Defaults
        {
            public static readonly ConfigValue<string> Directory =
                new ConfigValue<string>("directory",
                                        FARConfig.CombineGameData("..", "Logs", FARConfig.ModDirectoryName));

            public static readonly ConfigValue<string> NameFormat =
                new ConfigValue<string>("nameFormat", "<<<VESSEL_NAME>>>_<<<DATETIME>>>.csv");

            public static readonly ConfigValue<string> DatetimeFormat =
                new ConfigValue<string>("datetimeFormat", "yyyy_MM_dd_HH_mm_ss");

            public static readonly ConfigValue<int> Period = new ConfigValue<int>("period", 2);

            public static readonly ConfigValue<int> FlushPeriod = new ConfigValue<int>("flushPeriod", 10);
        }
    }
}
