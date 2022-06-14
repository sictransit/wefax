using CommandLine;
using Serilog;
using System.IO;

namespace net.sictransit.wefax
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(o =>
                  {
                      if (!File.Exists(o.SourceImage))
                      {
                          throw new FileNotFoundException(o.SourceImage);
                      }

                      var wavFilename = Path.Combine(Path.GetDirectoryName(o.SourceImage), $"{Path.GetFileNameWithoutExtension(o.SourceImage)}.wav");

                      var faxMachine = new FaxMachine(16000);

                      faxMachine.Fax(o.SourceImage, wavFilename, new BinaryCodedHeader(o.SatelliteName, o.SectorName, o.Date, o.Time, o.SectorName, o.Open));
                  });
        }

    }
}
