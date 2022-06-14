using SixLabors.ImageSharp.PixelFormats;

namespace net.sictransit.wefax.extensions
{
    internal static class PixelExtensions
    {
        public static float GetBrightness(this Rgb24 pixel)
        {
            float r = pixel.R / 255.0f;
            float g = pixel.G / 255.0f;
            float b = pixel.B / 255.0f;

            var max = r;
            var min = r;

            if (g > max)
            {
                max = g;
            }

            if (b > max)
            {
                max = b;
            }

            if (g < min)
            {
                min = g;
            }

            if (b < min)
            {
                min = b;
            }

            return (max + min) / 2;
        }

    }
}
