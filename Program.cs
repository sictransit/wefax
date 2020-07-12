using NAudio.Wave;
using Serilog;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace net.sictransit.wefax
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Send(@"img/EIA_Resolution_Chart_1956.png", new BCH("GC8WD6Q", "MST", DateTime.UtcNow, "JO89", "d: 1000 m, b: 90.00 deg"));            
        }

        private static void Send(string filename, BCH bch = null)
        {
            var fax = new Fax(16000);

            Log.Information($"loading: {filename}");

            using var original = Image.FromFile(filename);

            Log.Information($"original image: {original.Width}x{original.Height}");

            var scalingFactor = fax.ImageWidth / (float)original.Width;

            using var scaled = new Bitmap(fax.ImageWidth, (int)Math.Round(original.Height * scalingFactor));
            using var graphics = Graphics.FromImage(scaled);

            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(original, 0, 0, scaled.Width, scaled.Height);

            Log.Information($"scaled image: {scaled.Width}x{scaled.Height}");

            var wavFilename = Path.Combine(Path.GetDirectoryName(filename), $"{Path.GetFileNameWithoutExtension(filename)}.wav");

            using var writer = new WaveFileWriter(wavFilename, new WaveFormat(fax.SampleRate, 1));

            var start = fax.GetStart();

            writer.WriteSamples(start, 0, start.Length);

            var phasing = fax.GetPhasing();

            writer.WriteSamples(phasing, 0, phasing.Length);

            if (bch != null)
            {
                var header = fax.GetBCH(bch);

                writer.WriteSamples(header, 0, header.Length);

                var debug = fax.GetBCH(bch, true);

                writer.WriteSamples(debug, 0, debug.Length);

            }

            for (int y = 0; y < scaled.Height; y++)
            {
                var pixels = Enumerable.Range(0, scaled.Width).Select(x => scaled.GetPixel(x, y).GetBrightness() * 2f - 1).ToArray();

                var samples = fax.GetLine(pixels);

                writer.WriteSamples(samples, 0, samples.Length);
            }

            var stop = fax.GetStop();

            writer.WriteSamples(stop, 0, stop.Length);

            Log.Information($"wrote: {wavFilename}");
        }
    }
}
