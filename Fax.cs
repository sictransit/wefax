using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeFax
{
    class Fax
    {
        private const int Carrier = 1600;
        private readonly int lineLength;
        private readonly double dt;
        private readonly int deviation;
        private readonly int ioc;
        
        private double time = 0;

        public Fax(int sampleRate = 8000, int deviation = 400, int ioc = 576)
        {
            SampleRate = sampleRate;
            this.deviation = deviation;
            this.ioc = ioc;
            this.lineLength = sampleRate / 2;
            this.dt = Math.PI * 2 / sampleRate;
        }

        public double Resolution => (Math.PI * ioc);

        public int SampleRate { get; }

        public float[] GetPhasing()
        {
            var white = lineLength / 20;            
            var modulation = Enumerable.Range(0, lineLength).Select(x => x < white ? 1d : -1d).ToArray();

            return Enumerable.Range(0, 20 * 2).Select(_ => GetLine(modulation)).SelectMany(x => x).ToArray();
        }

        public float[] GetStart()
        {
            return GetSquareWave(300, 5);
        }

        public float[] GetStop()
        {
            return GetSquareWave(450, 5);
        }

        public float[] GetBCH(BCH bch)
        {
            var white = Enumerable.Range(0, lineLength / 20).Select(_ => 1d).ToArray();

            var bitLength = (lineLength - white.Length) / bch.Binary.Count() ;

            var one = Enumerable.Repeat(1d, bitLength).ToArray();
            var zero = Enumerable.Repeat(-1d, bitLength).ToArray();

            var bin = bch.Binary.Select(x => x == 1 ? one : zero).SelectMany(x => x).ToArray();            

            var padding = new double[lineLength - white.Length - bin.Length];

            return Enumerable.Range(0, bitLength / 4).Select(_ => GetLine(white.Concat(bin).Concat(padding).ToArray())).SelectMany(x => x).ToArray();
        }

        private float[] GetSquareWave(int frequency, int duration)
        {
            var modulation = Enumerable.Range(0, frequency).Select(x => x % 2 == 0 ? -1d : 1d).ToArray();            

            return Enumerable.Range(0, duration * 2).Select(_ => GetLine(modulation)).SelectMany(x => x).ToArray();
        }

        public float[] GetLine(double[] pixels = null)
        {
            //time = 0;

            if (pixels == null)
            {
                pixels = new double[1];
            }

            var lineLength = SampleRate / 2;

            var interpolationFactor = (double)pixels.Length / lineLength;

            var line = new float[lineLength];

            for (int i = 0; i < lineLength; i++)
            {
                var pixel = (int)(i * interpolationFactor);                

                var frequency = Carrier + deviation * pixels[pixel];

                time += dt * frequency;

                line[i] = (float)Math.Sin(time);                
            }

            return line;
        }
    }
}
