using MCOP.Modules.POE.Common;
using Serilog;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Services
{
    public struct SKText
    {
        public string Text { get; }
        public string HexColor { get; }

        public SKText(string text, string hexColor)
        {
            Text = text;
            HexColor = hexColor;
        }
    }

    public class SKTextLine : IEnumerable
    {
        public List<SKText> ColoredText { get; set; }

        public SKTextLine()
        {
            ColoredText = new List<SKText>();
        }
        public SKTextLine(IEnumerable<SKText> texts)
        {
            ColoredText = texts.ToList();
        }

        public void Add(SKText text)
        {
            ColoredText.Add(text);
        }
        public float GetWidth(SKPaint paint)
        {
            float width = 0;
            foreach (var item in ColoredText)
            {
                SKRect bounds = new SKRect();
                width += paint.MeasureText(item.Text, ref bounds);
            }
            return width + 8;
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)ColoredText).GetEnumerator();
        }
    }

    public static class ImageProcessorService
    {
        public static SKImage GetImageFromLines(params SKTextLine[] lines)
        {
            int lineHeight = 18;
            float maxWidth = 0;
            using var paint = new SKPaint();
            paint.Typeface = SKTypeface.FromFile("Fonts/Fontin-SmallCaps.otf");
            paint.TextSize = 14.0f;
            paint.IsAntialias = true;
            paint.Color = SKColors.White;
            foreach (var line in lines)
            {
                var lineWidth = line.GetWidth(paint);
                maxWidth = Math.Max(maxWidth, lineWidth);
            }
            var info = new SKImageInfo((int)maxWidth + 1, lineHeight * lines.Length);
            using var surface = SKSurface.Create(info);
            using SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Black);
            var offsetY = 13;
            foreach (var line in lines)
            {
                var offsetX = (info.Width - line.GetWidth(paint)) / 2.0f + 4;
                for (int i = 0; i < line.ColoredText.Count; i++)
                {
                    paint.Color = SKColor.Parse(line.ColoredText[i].HexColor);
                    SKRect bounds = new SKRect();
                    paint.MeasureText(line.ColoredText[i].Text, ref bounds);
                    canvas.DrawText(line.ColoredText[i].Text, offsetX, offsetY, paint);
                    offsetX += bounds.Width;
                }
                offsetY += lineHeight;
            }
            return surface.Snapshot();
        }
        public static Task GenerateImageAsync(this Armour item)
        {
            SKTextLine[] lines = new SKTextLine[]
            {
                new SKTextLine()
                {
                    new SKText("Quality:", "#827a6c"),
                    new SKText($" {item.Quality}%", "#8787fe")
                },
                new SKTextLine()
                {
                    new SKText("armour:", "#827a6c"),
                    new SKText($" {item.ArmourRating}", "#8787fe")
                },
                new SKTextLine()
                {
                    new SKText("evasion:", "#827a6c"),
                    new SKText($" {item.EvasionRating}", "#8787fe")
                },
                new SKTextLine()
                {
                    new SKText("energy shield:", "#827a6c"),
                    new SKText($" {item.EnergyShield}", "#8787fe")
                },

            };
            var image = GetImageFromLines(lines);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
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
