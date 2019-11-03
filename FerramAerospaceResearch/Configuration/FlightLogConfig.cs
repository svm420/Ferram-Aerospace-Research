namespace FerramAerospaceResearch
{
    // ReSharper disable once ClassNeverInstantiated.Global - instantiated through reflection
    [ConfigParser("flightLog")]
    public class FlightLogConfig : FARConfigParser<FlightLogConfig>
    {
        public StringConfigValue Directory { get; } = new StringConfigValue("directory",
                                                                            FARConfig.CombineKSPRoot("Logs",
                                                                                                     FARConfig
                                                                                                         .ModDirectoryName),
                                                                            FARConfig.CombineKSPRoot);

        public StringFormatterConfigValue NameFormat { get; } =
            new StringFormatterConfigValue("nameFormat", "<<<VESSEL_NAME>>>_<<<DATETIME>>>.csv");

        public StringConfigValue DatetimeFormat { get; } =
            new StringConfigValue("datetimeFormat", "yyyy_MM_dd_HH_mm_ss");

        public IntConfigValue Period { get; } = new IntConfigValue("period", 2);

        public IntConfigValue FlushPeriod { get; } = new IntConfigValue("flushPeriod", 10);
    }
}
