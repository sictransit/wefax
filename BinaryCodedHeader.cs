using System;
using System.Collections.Generic;
using System.Linq;

namespace net.sictransit.wefax
{
    //1- Satellite name(8 characters)          NOAA-11 
    //2- Spectral band(3 characters)          DIR
    //3- Date(YYMMDD)     (6 characters)          910125 
    //4- Time(HHMM)       (4 characters)          1315 
    //5- Sector name(4 Characters)          W020
    //6- Open(25 characters)         non-standard info

    public class BinaryCodedHeader
    {
        private readonly string satelliteName;
        private readonly string spectralBand;
        private readonly string sectorName;
        private readonly string open;
        private readonly string date;
        private readonly string time;

        public BinaryCodedHeader(string satelliteName, string spectralBand, string date, string time, string sectorName, string open)
        {
            this.satelliteName = TrimClipAndPad(satelliteName ?? string.Empty, 8);
            this.spectralBand = TrimClipAndPad(spectralBand ?? string.Empty, 3);
            this.date = TrimClipAndPad(date ?? string.Empty, 6);
            this.time = TrimClipAndPad(time ?? string.Empty, 4);
            this.sectorName = TrimClipAndPad(sectorName ?? string.Empty, 4);
            this.open = TrimClipAndPad(open ?? string.Empty, 25);
        }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Text);

        private static string TrimClipAndPad(string s, int length)
        {
            var trimmed = s.Trim();

            return trimmed.Substring(0, Math.Min(trimmed.Length, length)).PadRight(length);
        }

        public string Text => string.Concat(satelliteName, spectralBand, date, time, sectorName, open);

        public IEnumerable<int> Binary => Text.Select(c => Convert.ToString(c, 2).PadLeft(8, '0')).SelectMany(x => x).Select(c => c == '1' ? 1 : 0);
    }
}
