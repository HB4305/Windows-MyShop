using SkiaSharp;
using System.IO;

namespace MyShop.Utils;

public static class ImageHelper
{
    /// <summary>
    /// Resizes and compresses an image.
    /// </summary>
    public static byte[] CompressAndResize(byte[] imageBytes, int maxDimension = 800, int quality = 75)
    {
        try
        {
            using var inputStream = new MemoryStream(imageBytes);
            using var original = SKBitmap.Decode(inputStream);
            if (original == null) return imageBytes;

            int width = original.Width;
            int height = original.Height;

            if (width > maxDimension || height > maxDimension)
            {
                if (width > height)
                {
                    height = (int)(height * ((float)maxDimension / width));
                    width = maxDimension;
                }
                else
                {
                    width = (int)(width * ((float)maxDimension / height));
                    height = maxDimension;
                }
            }

            using var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
            if (resized == null) return imageBytes;

            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            return data.ToArray();
        }
        catch
        {
            return imageBytes;
        }
    }
}
