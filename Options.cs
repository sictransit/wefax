using CommandLine;

namespace net.sictransit.wefax
{
    internal class Options
    {
        [Option(Required = false, Default = "img/EIA_Resolution_Chart_1956.png", HelpText = "source image file to fax")]
        public string SourceImage { get; set; }

        [Option(Required = false, HelpText = "[BCH] satellite name (8 chars, e.g. \"NOAA-11\")")]
        public string SatelliteName { get; set; }

        [Option(Required = false, HelpText = "[BCH] spectral band (3 chars, e.g. \"DIR\")")]
        public string SpectralBane { get; set; }

        [Option(Required = false, HelpText = "[BCH] date (6 chars, \"yyMMdd\")")]
        public string Date { get; set; }

        [Option(Required = false, HelpText = "[BCH] time (4 chars, \"hhmm\")")]
        public string Time { get; set; }

        [Option(Required = false, HelpText = "[BCH] sector name (4 chars, e.g. \"W020\")")]
        public string SectorName { get; set; }

        [Option(Required = false, HelpText = "[BCH] non-standard info (25 chars)")]
        public string Open { get; set; }
    }
}
