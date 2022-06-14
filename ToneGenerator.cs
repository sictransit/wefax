using Serilog;
using System;
using System.Linq;

namespace net.sictransit.wefax
{
    public class ToneGenerator
    {
        private readonly int imageWidth;
        private readonly float[] whiteBar;
        private readonly int carrier;
        private readonly int deviation;
        private readonly int lineLength;

        private readonly double dt;
        private double time = 0;

        public ToneGenerator(int imageWidth, float[] whiteBar, int sampleRate, int carrier, int deviation)
        {
            this.imageWidth = imageWidth;
            this.whiteBar = whiteBar;
            this.carrier = carrier;
            this.deviation = deviation;

            lineLength = sampleRate / 2;
            dt = Math.PI * 2 / sampleRate;
        }

        public float[] GeneratePhasing()
        {
            var modulation = Enumerable.Range(0, imageWidth).Select(_ => -1f).ToArray();

            return Enumerable.Range(0, 20 * 2).Select(_ => GenerateLine(modulation)).SelectMany(x => x).ToArray();
        }

        public float[] GenerateStart()
        {
            return GenerateSquareWave(300, 5);
        }

        public float[] GenerateStop()
        {
            return GenerateSquareWave(450, 5);
        }

        public float[] GenerateBCH(BinaryCodedHeader bch, bool debug = false)
        {
            Log.Information($"encoding BCH: [{bch.Text}]");

            if (Log.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                var binary = bch.Binary.ToArray();

                for (int i = 0; i < bch.Text.Length; i++)
                {
                    var chunk = new string(binary.Skip(8 * i).Take(8).Select(x => x == 1 ? '1' : '0').ToArray());
                    var b = Convert.ToByte(chunk, 2);

                    Log.Debug($"decoded BCH: {(char)b} ({string.Concat(chunk.Select(x => x.ToString()).ToArray())})");
                }
            }

            var bitLength = 4;

            var one = Enumerable.Repeat(1f, bitLength).ToArray();
            var zero = Enumerable.Repeat(-1f, bitLength).ToArray();

            var bin = debug
                ? Enumerable.Range(0, bch.Binary.Count()).Select(x => x / 8 % 2 == 0 ? zero : one).SelectMany(x => x).ToArray()
                : bch.Binary.Select(x => x == 1 ? one : zero).SelectMany(x => x).ToArray();

            var padding = new float[imageWidth - bin.Length];

            return Enumerable.Range(0, bitLength).Select(_ => GenerateLine(bin.Concat(padding).ToArray())).SelectMany(x => x).ToArray();
        }

        public float[] GenerateLine(float[] pixels = null, bool bar = true)
        {
            if (pixels == null)
            {
                pixels = Array.Empty<float>();
            }

            var modulation = bar ? whiteBar.Concat(pixels).ToArray() : pixels;

            var interpolationFactor = (double)modulation.Length / lineLength;

            var line = new float[lineLength];

            for (int i = 0; i < lineLength; i++)
            {
                var pixel = (int)(i * interpolationFactor);

                var frequency = carrier + deviation * modulation[pixel];

                time += dt * frequency;

                line[i] = (float)Math.Sin(time);
            }

            return line;
        }

        private float[] GenerateSquareWave(int frequency, int duration)
        {
            var modulation = Enumerable.Range(0, frequency).Select(x => x % 2 == 0 ? -1f : 1f).ToArray();

            return Enumerable.Range(0, duration * 2).Select(_ => GenerateLine(modulation, false)).SelectMany(x => x).ToArray();
        }
    }
}
