using MCOP.Core.Models;
using SkiaSharp;

namespace MCOP.Core.Services.Image
{
    public class UserTopRendered
    {
        private const int RowHeight = 64;
        private const int AvatarSize = 40;
        private const int CellPadding = 8;
        private const int RowSpacing = 12;

        private readonly SKColor BackgroundColor = new(0x1E, 0x1F, 0x22);
        private readonly SKColor SecondaryColor = new(0x2B, 0x2D, 0x31);
        private readonly SKColor TextColor = SKColors.White;
        private readonly SKColor ProgressBackground = new(0x3E, 0x41, 0x48);

        private readonly HttpClient _httpClient = new();
        private readonly SKTypeface _typeface;
        private readonly SKTypeface _emojiTypeface;

        public UserTopRendered()
        {
            _typeface = SKTypeface.FromFamilyName("Arial");
            try
            {
                _emojiTypeface = SKTypeface.FromFamilyName("Segoe UI Emoji") ??
                                 SKTypeface.FromFamilyName("Apple Color Emoji") ??
                                 _typeface;
            }
            catch
            {
                _emojiTypeface = _typeface;
            }
        }

        public SKImage RenderTable(List<GuildUserStatsDto> users, int width)
        {
            var height = users.Count * (RowHeight + RowSpacing) + RowSpacing;
            var info = new SKImageInfo(width, height);

            using (var surface = SKSurface.Create(info))
            {
                var canvas = surface.Canvas;
                canvas.Clear(BackgroundColor);

                float y = RowSpacing;

                foreach (var user in users)
                {
                    RenderUserRow(canvas, user, y, width);
                    y += RowHeight + RowSpacing;
                }

                return surface.Snapshot();
            }
        }

        private void RenderUserRow(SKCanvas canvas, GuildUserStatsDto user, float y, float width)
        {
            // ---- Calculate column widths (unchanged) ----
            float fixedWidths = (AvatarSize + CellPadding * 2) + 200 + 100 + 100 + 100 + 16;
            float progressWidth = width - fixedWidths - (CellPadding * 2);

            float[] columnWidths = [
                AvatarSize + CellPadding * 2, // Avatar column
                200,                         // Username
                progressWidth,               // Level progress (dynamic)
                100,                         // Duel wins
                100,                         // Duel loses
                100                          // Likes
            ];

            // ---- Define the row rectangle ----
            float leftMargin = 16f;
            float totalWidth = columnWidths.Sum();
            var rowRect = new SKRect(leftMargin, y, leftMargin + totalWidth, y + RowHeight);

            // ---- Clip to rounded rectangle and draw row ----
            canvas.Save();
            using (var path = new SKPath())
            {
                float radius = 16f;
                path.AddRoundRect(rowRect, radius, radius);
                canvas.ClipPath(path, SKClipOperation.Intersect, true);
            }

            // Draw the row background (single rounded rect)
            using (var paint = new SKPaint { Color = SecondaryColor, IsAntialias = true })
            {
                canvas.DrawRect(rowRect, paint); // Fills the whole row, clipped to the rounded shape
            }

            // ---- Draw each cell’s content (without drawing cell backgrounds) ----
            float currentX = leftMargin;
            for (int i = 0; i < columnWidths.Length; i++)
            {
                var cellRect = new SKRect(currentX, y, currentX + columnWidths[i], y + RowHeight);

                switch (i)
                {
                    case 0: // Avatar
                        RenderAvatar(canvas, user, cellRect);
                        break;
                    case 1: // Username
                        RenderText(canvas, user.Username ?? "", cellRect, SKTextAlign.Left);
                        break;
                    case 2: // Level progress
                        RenderLevelProgress(canvas, user, cellRect);
                        break;
                    case 3: // Duel wins
                        RenderWithEmoji(canvas, "🎖️", user.DuelWin.ToString(), cellRect);
                        break;
                    case 4: // Duel loses
                        RenderWithEmoji(canvas, "☠️", user.DuelLose.ToString(), cellRect);
                        break;
                    case 5: // Likes
                        RenderWithEmoji(canvas, "❤️", user.Likes.ToString(), cellRect);
                        break;
                }

                currentX += columnWidths[i];
            }

            // Restore canvas (removes the clip)
            canvas.Restore();
        }

        private void RenderAvatar(SKCanvas canvas, GuildUserStatsDto user, SKRect rect)
        {
            try
            {
                var avatarUrl = GetDiscordAvatarUrl(user.UserId, user.AvatarHash ?? "");
                using (var imageStream = _httpClient.GetStreamAsync(avatarUrl).GetAwaiter().GetResult())
                using (var image = SKImage.FromEncodedData(SKData.Create(imageStream)))
                {
                    var avatarRect = new SKRect(
                        rect.MidX - AvatarSize / 2,
                        rect.MidY - AvatarSize / 2,
                        rect.MidX + AvatarSize / 2,
                        rect.MidY + AvatarSize / 2
                    );

                    canvas.Save();

                    using (var path = new SKPath())
                    {
                        path.AddCircle(rect.MidX, rect.MidY, AvatarSize / 2);
                        canvas.ClipPath(path, SKClipOperation.Intersect, true);
                    }

                    canvas.DrawImage(image, avatarRect);
                    canvas.Restore();
                }
            }
            catch
            {
                using (var paint = new SKPaint { Color = SKColors.Gray })
                {
                    canvas.DrawCircle(rect.MidX, rect.MidY, AvatarSize / 2, paint);
                }
            }
        }

        private void RenderText(SKCanvas canvas, string text, SKRect rect, SKTextAlign align)
        {
            using (var paint = new SKPaint
            {
                Color = TextColor,
                IsAntialias = true,
            })
            {
                var skFont = new SKFont(_typeface, size: 14);
                skFont.MeasureText(text, out SKRect textBounds, paint);

                float x = align == SKTextAlign.Left ?
                    rect.Left + CellPadding :
                    align == SKTextAlign.Right ?
                    rect.Right - CellPadding :
                    rect.MidX;

                float y = rect.MidY + textBounds.Height / 2;

                canvas.DrawText(text, x, y, align, skFont, paint);
            }
        }

        private void RenderLevelProgress(SKCanvas canvas, GuildUserStatsDto user, SKRect rect)
        {
            var progressHeight = 6;
            var progressRect = new SKRect(
                rect.Left + 100,
                rect.Bottom - CellPadding - progressHeight,
                rect.Right - 100,
                rect.Bottom - CellPadding
            );

            var textRect = new SKRect(
                progressRect.Left,
                rect.Top,
                progressRect.Right,
                rect.Bottom
            );
            RenderText(canvas, $"Level {user.Level}", textRect, SKTextAlign.Center);

            using (var paint = new SKPaint { Color = ProgressBackground, IsAntialias = true })
            {
                canvas.DrawRoundRect(progressRect, progressHeight / 2, progressHeight / 2, paint);
            }

            float progress = (float)(user.Exp - user.CurrentLevelExp) / (user.NextLevelExp - user.CurrentLevelExp);
            progress = Math.Clamp(progress, 0, 1);

            var fillRect = new SKRect(
                progressRect.Left,
                progressRect.Top,
                progressRect.Left + progressRect.Width * progress,
                progressRect.Bottom
            );

            if (fillRect.Width > 0)
            {
                var startColor = new SKColor(0x63, 0x66, 0xF1); // rgb(99,102,241)
                var endColor = new SKColor(0x06, 0xB6, 0xD4); // rgb(6,182,212)

                // Create a horizontal gradient (left → right)
                using (var shader = SKShader.CreateLinearGradient(
                    new SKPoint(fillRect.Left, fillRect.Top),
                    new SKPoint(fillRect.Right, fillRect.Top),
                    [startColor, endColor],
                    null,
                    SKShaderTileMode.Clamp))
                using (var paint = new SKPaint
                {
                    Shader = shader,
                    IsAntialias = true
                })
                {
                    canvas.DrawRoundRect(fillRect, progressHeight / 2, progressHeight / 2, paint);
                }
            }
        }

        private void RenderWithEmoji(SKCanvas canvas, string emoji, string text, SKRect rect)
        {
            using (var paint = new SKPaint
            {
                Color = TextColor,
                IsAntialias = true,
            })
            {
                float y = rect.MidY + 14 / 2;
                canvas.DrawText(emoji, rect.Left + CellPadding, y, SKTextAlign.Left, new SKFont(_emojiTypeface, size: 14), paint);
            }
            RenderText(canvas, text, new SKRect(rect.Left + 24, rect.Top, rect.Right, rect.Bottom), SKTextAlign.Left);
        }

        private string GetDiscordAvatarUrl(string userId, string avatarHash)
        {
            return string.IsNullOrEmpty(avatarHash) ?
                $"https://cdn.discordapp.com/embed/avatars/{ulong.Parse(userId) % 5}.png" :
                $"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.png?size=64";
        }
    }
}