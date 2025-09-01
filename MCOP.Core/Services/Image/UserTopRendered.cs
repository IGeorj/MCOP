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

        private readonly SKColor BackgroundColor = new SKColor(0x1E, 0x1F, 0x22);
        private readonly SKColor SecondaryColor = new SKColor(0x2B, 0x2D, 0x31);
        private readonly SKColor TextColor = SKColors.White;
        private readonly SKColor ProgressBackground = new SKColor(0x3E, 0x41, 0x48);
        private readonly SKColor ProgressFill = new SKColor(0x58, 0x65, 0xD2);

        private readonly HttpClient _httpClient = new HttpClient();
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
            float fixedWidths = (AvatarSize + CellPadding * 2) + 200 + 100 + 100 + 100 + 16;
            float progressWidth = width - fixedWidths - (CellPadding * 2);

            float[] columnWidths = [
                AvatarSize + CellPadding * 2, // Avatar
                200,                         // Username
                progressWidth,               // Level progress (dynamic)
                100,                        // Duel wins
                100,                        // Duel loses
                100                         // Likes
            ];

            float x = 16f;

            for (int i = 0; i < columnWidths.Length; i++)
            {
                var isFirst = i == 0;
                var isLast = i == columnWidths.Length - 1;

                using (var path = new SKPath())
                {
                    var rect = new SKRect(x, y, x + columnWidths[i], y + RowHeight);

                    path.AddRoundRect(new SKRoundRect(rect));

                    using (var paint = new SKPaint { Color = SecondaryColor })
                    {
                        canvas.DrawPath(path, paint);
                    }

                    // Draw cell content
                    switch (i)
                    {
                        case 0: // Avatar
                            RenderAvatar(canvas, user, rect);
                            break;
                        case 1: // Username
                            RenderText(canvas, user.Username, rect, SKTextAlign.Left);
                            break;
                        case 2: // Level progress
                            RenderLevelProgress(canvas, user, rect);
                            break;
                        case 3: // Duel wins
                            RenderWithEmoji(canvas, "🎖️", user.DuelWin.ToString(), rect);
                            break;
                        case 4: // Duel loses
                            RenderWithEmoji(canvas, "☠️", user.DuelLose.ToString(), rect);
                            break;
                        case 5: // Likes
                            RenderWithEmoji(canvas, "❤️", user.Likes.ToString(), rect);
                            break;
                    }
                }

                x += columnWidths[i];
            }
        }

        private void RenderAvatar(SKCanvas canvas, GuildUserStatsDto user, SKRect rect)
        {
            try
            {
                var avatarUrl = GetDiscordAvatarUrl(user.UserId, user.AvatarHash);
                using (var imageStream = _httpClient.GetStreamAsync(avatarUrl).Result)
                using (var image = SKImage.FromEncodedData(SKData.Create(imageStream)))
                {
                    var avatarRect = new SKRect(
                        rect.MidX - AvatarSize / 2,
                        rect.MidY - AvatarSize / 2,
                        rect.MidX + AvatarSize / 2,
                        rect.MidY + AvatarSize / 2
                    );

                    canvas.DrawImage(image, avatarRect);
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
                Typeface = _typeface,
                TextSize = 14,
                IsAntialias = true,
                TextAlign = align
            })
            {
                var textBounds = new SKRect();
                paint.MeasureText(text, ref textBounds);

                float x = align == SKTextAlign.Left ?
                    rect.Left + CellPadding :
                    align == SKTextAlign.Right ?
                    rect.Right - CellPadding :
                    rect.MidX;

                float y = rect.MidY + textBounds.Height / 2;

                canvas.DrawText(text, x, y, paint);
            }
        }

        private void RenderLevelProgress(SKCanvas canvas, GuildUserStatsDto user, SKRect rect)
        {
            RenderText(canvas, $"Level {user.Level}", rect, SKTextAlign.Center);

            var progressHeight = 6;
            var progressRect = new SKRect(
                rect.Left + 100,
                rect.Bottom - CellPadding - progressHeight,
                rect.Right - 100,
                rect.Bottom - CellPadding
            );

            using (var paint = new SKPaint { Color = ProgressBackground })
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

            using (var paint = new SKPaint { Color = ProgressFill })
            {
                canvas.DrawRoundRect(fillRect, progressHeight / 2, progressHeight / 2, paint);
            }
        }

        private void RenderWithEmoji(SKCanvas canvas, string emoji, string text, SKRect rect)
        {
            using (var paint = new SKPaint
            {
                Color = TextColor,
                Typeface = _emojiTypeface,
                TextSize = 14,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left
            })
            {
                float y = rect.MidY + paint.TextSize / 2;
                canvas.DrawText(emoji, rect.Left + CellPadding, y, paint);
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