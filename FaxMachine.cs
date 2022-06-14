using NAudio.Wave;
using net.sictransit.wefax.extensions;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Linq;

namespace net.sictransit.wefax
{
    public class FaxMachine
    {

        private readonly int sampleRate;
        private readonly int carrier;
        private readonly int deviation;
        private readonly int resolution;
        private readonly float[] whiteBar;

        public FaxMachine(int sampleRate = 8000, int carrier = 1600, int deviation = 400, int ioc = 576)
        {
            this.sampleRate = sampleRate;
            this.carrier = carrier;
            this.deviation = deviation;

            resolution = (int)(Math.PI * ioc);
            whiteBar = Enumerable.Repeat(1f, resolution / 20).ToArray();
        }

        public void Fax(string imageFilename, string audioFilename, BinaryCodedHeader bch)
        {
            Log.Information($"image input: {imageFilename}");

            using var image = Image.Load<Rgb24>(imageFilename);

            Log.Information($"original image: {image.Width}x{image.Height}");

            var imageWidth = resolution - whiteBar.Length;

            var scalingFactor = imageWidth / (float)image.Width;

            image.Mutate(i => i.Resize((int)Math.Round(image.Width * scalingFactor), (int)Math.Round(image.Height * scalingFactor)));

            Log.Information($"scaled image: {image.Width}x{image.Height}");

            using var writer = new WaveFileWriter(audioFilename, new WaveFormat(sampleRate, 1));

            Log.Information($"audio output: {audioFilename}");

            var toneGenerator = new ToneGenerator(imageWidth, whiteBar, sampleRate, carrier, deviation);

            var start = toneGenerator.GenerateStart();

            writer.WriteSamples(start, 0, start.Length);

            var phasing = toneGenerator.GeneratePhasing();

            writer.WriteSamples(phasing, 0, phasing.Length);

            if (!bch.IsEmpty)
            {
                var header = toneGenerator.GenerateBCH(bch);

                writer.WriteSamples(header, 0, header.Length);
            }

            for (int y = 0; y < image.Height; y++)
            {
                var pixels = Enumerable.Range(0, image.Width).Select(x => image[x, y].GetBrightness() * 2f - 1).ToArray();

                var samples = toneGenerator.GenerateLine(pixels);

                writer.WriteSamples(samples, 0, samples.Length);
            }

            var stop = toneGenerator.GenerateStop();

            writer.WriteSamples(stop, 0, stop.Length);
        }


    }
}
