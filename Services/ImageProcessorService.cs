using MCOP.Modules.POE.Common;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Services
{
    public static class ImageProcessorService
    {
        public static SKImage CreateLine(string text, string value)
        {
            var info = new SKImageInfo(400, 18);
            using var surface = SKSurface.Create(info);
            using SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Black);
            using var font = SKTypeface.FromFile("Fonts/Fontin-SmallCaps.otf");
            using var textPaint = new SKPaint();
            using var valuePaint = new SKPaint();
            textPaint.Typeface = font;
            textPaint.TextSize = 14.0f;
            textPaint.IsAntialias = true;
            textPaint.Color = SKColor.Parse("#827a6c");
            valuePaint.Typeface = font;
            valuePaint.TextSize = 14.0f;
            valuePaint.IsAntialias = true;
            valuePaint.Color = SKColor.Parse("#8787fe");
            SKRect textBounds = new SKRect();
            SKRect valueBounds = new SKRect();
            textPaint.MeasureText(text, ref textBounds);
            valuePaint.MeasureText(value, ref valueBounds);
            var offsetX = (info.Width - (textBounds.Width + valueBounds.Width)) / 2.0f;
            canvas.DrawText(text, offsetX, 11, textPaint);
            canvas.DrawText(value, offsetX + textBounds.Width, 11, valuePaint);
            return surface.Snapshot();
        }

        public static Task GenerateImageAsync(this Armour item)
        {
            using var qualityImage = CreateLine("Quality:", $" {item.Quality}%");
            using var armourImage = CreateLine("armour:", $" {item.ArmourRating}");
            using var evasionImage = CreateLine("evasion:", $" {item.EvasionRating}");
            using var energyShieldImage = CreateLine("energy shield:", $" {item.EnergyShield}");
            var info = new SKImageInfo(qualityImage.Width,
                qualityImage.Height + armourImage.Height + evasionImage.Height + energyShieldImage.Height);
            using var surface = SKSurface.Create(info);
            using var canvas = surface.Canvas;
            canvas.DrawImage(qualityImage, 0f, 0f, null);
            canvas.DrawImage(armourImage, 0f, qualityImage.Height, null);
            canvas.DrawImage(evasionImage, 0f, qualityImage.Height + armourImage.Height, null);
            canvas.DrawImage(energyShieldImage, 0f, qualityImage.Height + armourImage.Height + evasionImage.Height, null);
            using var snapshot = surface.Snapshot();
            using var data = snapshot.Encode();
            using var stream = File.OpenWrite("Images/POE/generate.png");
            data.SaveTo(stream);
            
            return Task.CompletedTask;
        }

        public static Task<bool> SaveAsJpgAsync(SKBitmap bitmap, string path, int quality)
        {
            try
            {
                using (SKImage image = SKImage.FromBitmap(bitmap))
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
            catch (Exception)
            {
                Log.Error("SkiaSharp failed to save image as jpg. Path: {path}", path);
                return Task.FromResult(true);
            }
        }

        public static Task<bool> SaveAsJpgAsync(byte[] bytes, string path, int quality)
        {
            using SKBitmap bitmap = SKBitmap.Decode(bytes);
            return SaveAsJpgAsync(bitmap, path, quality);

        }

        public static Task<bool> SaveAsJpgAsync(string pathFrom, string pathTo, int quality)
        {
            using (FileStream fstream = File.OpenRead(pathFrom))
            {
                using (SKBitmap bitmap = SKBitmap.Decode(fstream))
                {
                    return SaveAsJpgAsync(bitmap, pathTo, quality);
                }
            }
        }



        public static double GetPercentageDifference(SKBitmap img1, SKBitmap img2, int threshold = 3)
        {
            return GetPercentageDifference(GetBitmapHash(img1), GetBitmapHash(img2), threshold);
        }

        public static double GetPercentageDifference(byte[] img1, byte[] img2, int threshold = 3)
        {
            byte[] differences = GetDifferences(img1, img2);

            int numberOfPixels = differences.Length;
            int diffAmount = differences.Count(p => p < threshold);

            double proc = (double)diffAmount / (double)numberOfPixels * 100;

            return Math.Round(proc, 2);
        }
        
        public static byte[] GetBitmapHash(this SKBitmap bitmap, int arraySize = 16)
        {
            bitmap = GrayScaleBitmap(bitmap);
            bitmap = ResizeBitmap(bitmap, 16, 16);

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
            return bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
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
