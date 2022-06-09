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
        public string Text { get; set; }
        public string HexColor { get; set; }
        public float FontSize { get; set; } = 14.0f;

        public SKText(string text, string hexColor)
        {
            Text = text;
            HexColor = hexColor;
        }

        public SKText(string text, string hexColor, float fontSize) : this(text, hexColor)
        {
            FontSize = fontSize;
        }
    }

    public class SKTextLine : IEnumerable
    {
        public List<SKText> ColoredText { get; }
        public float Width = 0;
        public float Height = 0;
        public int Padding = 4;
        private SKTypeface Typeface { get; }

        public SKTextLine()
        {
            Typeface = SKTypeface.FromFile("Fonts/Fontin-SmallCaps.otf");
            ColoredText = new List<SKText>();
        }

        public void Add(IEnumerable<SKText> texts)
        {
            using SKPaint paint = new SKPaint();
            paint.Typeface = Typeface;
            paint.IsAntialias = true;
            float maxWidth = 0;
            float maxHeight = 0;
            foreach (var item in texts)
            {
                ColoredText.Add(item);
                SKRect bounds = new SKRect();
                paint.TextSize = item.FontSize;
                paint.Color = SKColor.Parse(item.HexColor);
                maxWidth += paint.MeasureText(item.Text, ref bounds);
                maxHeight = Math.Max(maxHeight, item.FontSize);
            }
            Width = maxWidth;
            Height = maxHeight + Padding;
        }

        public void Add(SKText text)
        {
            ColoredText.Add(text);
            using SKPaint paint = new SKPaint();
            paint.Typeface = Typeface;
            paint.IsAntialias = true;
            paint.TextSize = text.FontSize;
            paint.Color = SKColor.Parse(text.HexColor);
            SKRect bounds = new SKRect();
            Width += paint.MeasureText(text.Text, ref bounds);
            Height = Math.Max(Height, text.FontSize + Padding);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)ColoredText).GetEnumerator();
        }
    }

    public static class ImageProcessorService
    {
        public static SKImage GetImageFromLines(IEnumerable<SKTextLine> lines)
        {
            float imageWidth = 0;
            float imageHeight = 0;
            int Padding = 8;
            using var paint = new SKPaint();
            paint.Typeface = SKTypeface.FromFile("Fonts/Fontin-SmallCaps.otf");
            paint.IsAntialias = true;
            foreach (var line in lines)
            {
                imageWidth = Math.Max(imageWidth, line.Width);
                imageHeight += line.Height;
            }

            var info = new SKImageInfo((int)imageWidth + Padding + 1 , (int)imageHeight + Padding + 1);
            using var surface = SKSurface.Create(info);
            using SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Black);
            float offsetY = 0;
            foreach (var line in lines)
            {
                offsetY += line.Height;
                float offsetX = (info.Width - line.Width) / 2.0f;
                for (int i = 0; i < line.ColoredText.Count; i++)
                {
                    paint.Color = SKColor.Parse(line.ColoredText[i].HexColor);
                    paint.TextSize = line.ColoredText[i].FontSize;
                    SKRect bounds = new SKRect();
                    var width = paint.MeasureText(line.ColoredText[i].Text, ref bounds);
                    canvas.DrawText(line.ColoredText[i].Text, offsetX, offsetY, paint);
                    offsetX += width;
                }
            }
            return surface.Snapshot();
        }
        public static string GenerateImage(this Armour item)
        {
            List<SKTextLine> lines = new List<SKTextLine>();
            lines.Add(item.NameToText());
            var quality = item.QualityToText();
            var armour = item.ArmourRatingToText();
            var evasion = item.EvasionRatingToText();
            var energyShield = item.EnergyShieldToText();
            if (quality is not null)
            {
                lines.Add(quality);
            }
            if (armour is not null)
            {
                lines.Add(armour);
            }
            if (evasion is not null)
            {
                lines.Add(evasion);
            }
            if (energyShield is not null)
            {
                lines.Add(energyShield);
            }
            lines.Add(item.ItemLevelToText());
            if (item.StatsRequirements is not null)
            {
                lines.Add(item.StatsRequirements.StatsToTextLine());
            }

            string path = "Images/POE/generate.png";

            SKBitmap bitmapHeader;
            using (Stream loadStream = File.OpenRead(item.Image))
            {
                bitmapHeader = SKBitmap.Decode(loadStream);
            }
            bitmapHeader = bitmapHeader.Resize(new SKImageInfo(bitmapHeader.Width / 2, bitmapHeader.Height / 2), SKFilterQuality.High);

            var imageText = GetImageFromLines(lines);
            var leftPadding = 8;
            var info = new SKImageInfo(imageText.Width + bitmapHeader.Width + leftPadding, Math.Max(imageText.Height, bitmapHeader.Height));
            using var surface = SKSurface.Create(info);
            using SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Black);
            float offsetX = leftPadding;
            float offsetY = (info.Height - imageText.Height) / 2.0f;
            canvas.DrawImage(imageText, offsetX, offsetY);
            offsetX += imageText.Width;
            offsetY = (info.Height - bitmapHeader.Height) / 2.0f;
            canvas.DrawBitmap(bitmapHeader, offsetX, offsetY);

            using var data = surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(path);
            data.SaveTo(stream);
            
            return path;
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
