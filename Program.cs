using NAudio.Wave;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace WeFax
{
    class Program
    {
        static void Main(string[] args)
        {
            Send(@"C:\Users\micke\Dropbox\GC\!hidden\GC8WD6Q - WEFAX (bonus #3)\calibration.png", new BCH("GC8WD6Q", "MST", DateTime.UtcNow, "JO89", "d: 5102 m, b: 37.07 deg"));
            Send(@"C:\Users\micke\Dropbox\GC\!hidden\GC8WD6Q - WEFAX (bonus #3)\map.png");
        }

        private static void Send(string filename, BCH bch = null)
        {
            var fax = new Fax(16000);

            using var original = Bitmap.FromFile(filename);

            var borderWidth = (int)fax.Resolution / 20;
            var imageWidth = (int)fax.Resolution - borderWidth;

            var scalingFactor = (fax.Resolution - borderWidth) / original.Width;

            var faxWidth = borderWidth + imageWidth;
            var faxHeight = original.Height * scalingFactor;

            using var scaled = new Bitmap(faxWidth, (int)Math.Round(faxHeight));
            using var graphics = Graphics.FromImage(scaled);
            using var whiteBrush = new SolidBrush(Color.White);
            graphics.FillRectangle(whiteBrush, 0, 0, scaled.Width, scaled.Height);

            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(original, (int)borderWidth, 0, (int)imageWidth, (int)faxHeight);

            scaled.Save("scaled.png", ImageFormat.Png);

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
            }

            for (int y = 0; y < scaled.Height; y++)
            {
                var pixels = Enumerable.Range(0, scaled.Width).Select(x => scaled.GetPixel(x, y).GetBrightness() * 2d - 1).ToArray();

                var samples = fax.GetLine(pixels);

                writer.WriteSamples(samples, 0, samples.Length);
            }

            var stop = fax.GetStop();

            writer.WriteSamples(stop, 0, stop.Length);

            Console.WriteLine($"{scaled.Width}x{scaled.Height}");
        }
    }
}
