using System.Collections.Generic;
using System.IO;
using System.Text;
using FerramAerospaceResearch.FARUtils;

namespace FerramAerospaceResearch
{
    // ReSharper disable once ClassNeverInstantiated.Global - instantiated through reflection
    [ConfigParserAttribute("flightLog")]
    public class FlightLogConfig : FARConfigParser<FlightLogConfig>
    {
        public static readonly ConfigValue<string> DirectoryDefault =
            new ConfigValue<string>("directory", Path.Combine(FARConfig.FARRootPath, "logs"));

        public static readonly ConfigValue<string> NameFormatDefault =
            new ConfigValue<string>("nameFormat", "<<<VESSEL_NAME>>>_<<<DATETIME>>>.csv");

        public static readonly ConfigValue<string> DatetimeFormatDefault =
            new ConfigValue<string>("datetimeFormat", "yyyy_MM_dd_HH_mm_ss");

        public static readonly ConfigValue<int> PeriodDefault = new ConfigValue<int>("period", 2);

        public static readonly ConfigValue<int> FlushPeriodDefault = new ConfigValue<int>("flushPeriod", 10);

        private string directory;
        private string datetimeFormat;

        public string Directory
        {
            get { return directory; }
        }

        public string DatetimeFormat
        {
            get { return datetimeFormat; }
        }

        private int period;
        private int flushPeriod;

        public int Period
        {
            get { return period; }
        }

        public int FlushPeriod
        {
            get { return flushPeriod; }
        }

        public StringFormatter NameFormat { get; } = new StringFormatter(NameFormatDefault.Value);

        public override void Reset()
        {
            directory = DirectoryDefault.Value;
            datetimeFormat = DatetimeFormatDefault.Value;
            NameFormat.FormatString = NameFormatDefault.Value;
            period = PeriodDefault.Value;
            flushPeriod = FlushPeriodDefault.Value;
        }

        public override void Parse(IConfigNode node)
        {
            if (node.TryGetValue(DirectoryDefault.Name, ref directory))
                if (!Path.IsPathRooted(directory))
                    directory = Path.Combine(FARConfig.KSPRootPath, directory);

            node.TryGetValue(DatetimeFormatDefault.Name, ref datetimeFormat);

            string str = string.Empty;
            if (node.TryGetValue(NameFormatDefault.Name, ref str))
                NameFormat.FormatString = str;

            node.TryGetValue(PeriodDefault.Name, ref period);
            node.TryGetValue(FlushPeriodDefault.Name, ref flushPeriod);
        }

        public override void SaveTo(IConfigNode node)
        {
            node.AddValue($"%{DirectoryDefault.Name}", directory);
            node.AddValue($"%{NameFormatDefault.Name}", NameFormat.FormatString);
            node.AddValue($"%{DatetimeFormatDefault.Name}", datetimeFormat);
            node.AddValue($"%{PeriodDefault.Name}", period);
            node.AddValue($"%{FlushPeriodDefault.Name}", flushPeriod);
        }

        public override void DebugString(StringBuilder sb)
        {
            AppendEntry(sb, DirectoryDefault.Name, directory);
            AppendEntry(sb, NameFormatDefault.Name, NameFormat.FormatString);
            AppendEntry(sb, DatetimeFormatDefault.Name, datetimeFormat);
            AppendEntry(sb, PeriodDefault.Name, period);
            AppendEntry(sb, FlushPeriodDefault.Name, flushPeriod);
        }
    }
}
