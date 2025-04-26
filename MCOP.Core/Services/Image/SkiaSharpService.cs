using Serilog;
using SkiaSharp;

namespace MCOP.Core.Services.Image
{
    public static class SkiaSharpService
    {
        public static Task<bool> SaveAsJpgAsync(SKBitmap bitmap, string path, int quality, decimal resizeRation = 0)
        {
            try
            {
                if (resizeRation != 0)
                {
                    bitmap = bitmap.Resize(new SKImageInfo((int)(bitmap.Width / resizeRation), (int)(bitmap.Height / resizeRation)), new SKSamplingOptions(SKCubicResampler.Mitchell));
                }
                var info = new SKImageInfo(bitmap.Width, bitmap.Height);
                using (SKSurface surface = SKSurface.Create(info))
                {
                    using (SKCanvas canvas = surface.Canvas)
                    {
                        canvas.Clear(SKColors.White);
                        canvas.DrawBitmap(bitmap, 0, 0);
                        using (SKImage image = surface.Snapshot())
                        {
                            using (SKData data = image.Encode(SKEncodedImageFormat.Jpeg, quality))
                            {
                                using (var stream = File.OpenWrite(path))
                                {
                                    data.SaveTo(stream);
                                    return Task.FromResult(true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Log.Error("SkiaSharp failed to save image as jpg. Path: {path}", path);
                return Task.FromResult(false);
            }
        }

        public static Task<bool> SaveAsJpgAsync(byte[] bytes, string path, int quality)
        {
            using SKBitmap bitmap = SKBitmap.Decode(bytes);
            return SaveAsJpgAsync(bitmap, path, quality);
        }

        public static Task<bool> SaveAsJpgAsync(string pathFrom, string pathTo, int quality, decimal resizeRation = 0)
        {
            using (FileStream fstream = File.OpenRead(pathFrom))
            {
                using (SKBitmap bitmap = SKBitmap.Decode(fstream))
                {
                    return SaveAsJpgAsync(bitmap, pathTo, quality, resizeRation);
                }
            }
        }

        public static double GetPercentageDifference(SKBitmap img1, SKBitmap img2, int threshold = 3)
        {
            return GetPercentageDifference(GetBitmapHash(img1), GetBitmapHash(img2), threshold);
        }

        public static double GetNormalizedDifference(SKBitmap img1, SKBitmap img2)
        {
            return GetNormalizedDifference(GetBitmapHash(img1), GetBitmapHash(img2));
        }

        public static double GetPercentageDifference(byte[] img1, byte[] img2, int threshold = 3)
        {
            byte[] differences = GetDifferences(img1, img2);

            int numberOfPixels = differences.Length;
            int diffAmount = differences.Count(p => p < threshold);

            double proc = (double)diffAmount / (double)numberOfPixels * 100;

            return proc;
        }

        public static double GetNormalizedDifference(byte[] img1, byte[] img2)
        {
            int correlation = 0;
            double denominator = 0;

            for (int i = 0; i < img1.Length; i++)
            {
                correlation += img1[i] * img2[i];
            }

            denominator = Math.Sqrt(img1.Sum(x => Math.Pow(x, 2)) * img2.Sum(x => Math.Pow(x, 2)));
            return (correlation / denominator) * 100;
        }

        public static byte[] GetBitmapHash(SKBitmap bitmap, int arraySize = 32)
        {
            bitmap = GrayScaleBitmap(bitmap);
            bitmap = ResizeBitmap(bitmap, arraySize, arraySize);

            byte[] grayScale = new byte[arraySize * arraySize];

            int index = 0;
            foreach (var pixel in bitmap.Pixels)
            {
                grayScale[index] = (byte)Math.Abs(pixel.Red);
                index++;
            }

            return grayScale;
        }

        public static SKBitmap GrayScaleBitmap(SKBitmap bitmap)
        {
            SKImageInfo info = bitmap.Info;
            SKSurface surface = SKSurface.Create(info);
            SKCanvas canvas = surface.Canvas;
            SKBitmap resultBitmap;

            using (SKPaint paint = new())
            {
                paint.ColorFilter =
                    SKColorFilter.CreateColorMatrix(new float[]
                    {
                        0.21f, 0.72f, 0.07f, 0, 0,
                        0.21f, 0.72f, 0.07f, 0, 0,
                        0.21f, 0.72f, 0.07f, 0, 0,
                        0,     0,     0,     1, 0
                    });

                canvas.DrawBitmap(bitmap, info.Rect, paint: paint);
                SKImage sanpshot = surface.Snapshot();
                resultBitmap = SKBitmap.Decode(sanpshot.Encode());
            }

            return resultBitmap;
        }

        public static SKBitmap ResizeBitmap(SKBitmap bitmap, int width, int height)
        {
            return bitmap.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKCubicResampler.Mitchell));
        }


        private static byte[] GetDifferences(byte[] firstGray, byte[] secondGray)
        {
            byte[] differences = new byte[firstGray.Length];

            for (int i = 0; i < firstGray.Length; i++)
            {
                differences[i] = (byte)Math.Abs(firstGray[i] - secondGray[i]);
            }
            return differences;
        }
    }
}
