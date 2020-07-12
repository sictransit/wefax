using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeFax
{
  //1- Satellite name(8 characters)          NOAA-11 
  //2- Spectral band(3 characters)          DIR
  //3- Date(YYMMDD)     (6 characters)          910125 
  //4- Time(HHMM)       (4 characters)          1315 
  //5- Sector name(4 Characters)          W020
  //6- Open(25 characters)         non-standard info


    class BCH
    {
        private readonly string satelliteName;
        private readonly string spectralBand;
        private readonly DateTime timestamp;
        private readonly string sectorName;
        private readonly string open;

        public BCH(string satelliteName, string spectralBand, DateTime timestamp, string sectorName, string open)
        {
            this.satelliteName = ClipAndPad(satelliteName ?? string.Empty, 8);
            this.spectralBand = ClipAndPad(spectralBand ?? string.Empty, 3);
            this.timestamp = timestamp;
            this.sectorName = ClipAndPad(sectorName ?? string.Empty, 4);
            this.open = ClipAndPad(open ?? string.Empty, 25);
        }

        private string ClipAndPad(string s, int length)
        {
            return s.Substring(0, Math.Min(s.Length, length)).PadRight(length);
        }

        public string Text => string.Concat(satelliteName, spectralBand, timestamp.ToString("yyMMdd"), timestamp.ToString("HHmm"), sectorName, open);

        public IEnumerable<int> Binary => Text.Select(c => Convert.ToString(c, 2).PadLeft(8, '0')).SelectMany(x => x).Select(c => c == '1' ? 1 : 0);
    }
}
