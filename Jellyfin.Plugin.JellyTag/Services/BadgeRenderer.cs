using SkiaSharp;

namespace Jellyfin.Plugin.JellyTag.Services;

/// <summary>
/// Service for rendering badges on media images using SkiaSharp.
/// </summary>
public class BadgeRenderer : IDisposable
{
    private readonly Dictionary<string, SKBitmap> _badgeCache = new();
    private bool _disposed;

    /// <summary>
    /// Renders badges on an image based on metadata.
    /// </summary>
    /// <param name="originalImage">The original image bitmap.</param>
    /// <param name="metadata">The parsed metadata.</param>
    /// <param name="position">Badge position (TopLeft, TopRight, BottomLeft, BottomRight).</param>
    /// <param name="badgeSize">Badge size percentage (50-200).</param>
    /// <param name="enableResolution">Enable resolution badges.</param>
    /// <param name="enableHdr">Enable HDR badges.</param>
    /// <param name="enableCodec">Enable codec badges.</param>
    /// <param name="enableAudio">Enable audio badges.</param>
    /// <param name="enableSource">Enable source badges.</param>
    /// <returns>A new bitmap with badges rendered.</returns>
    public SKBitmap RenderBadges(
        SKBitmap originalImage,
        MediaMetadata metadata,
        string position = "TopRight",
        int badgeSize = 100,
        bool enableResolution = true,
        bool enableHdr = true,
        bool enableCodec = true,
        bool enableAudio = true,
        bool enableSource = true)
    {
        // Create a new bitmap to draw on
        var result = new SKBitmap(originalImage.Width, originalImage.Height);
        using var canvas = new SKCanvas(result);

        // Draw the original image
        canvas.DrawBitmap(originalImage, 0, 0);

        // Collect badges to render
        var badges = new List<string>();

        if (enableResolution && metadata.Resolution != null)
        {
            badges.Add(metadata.Resolution);
        }

        if (enableHdr && metadata.HdrFormats.Count > 0)
        {
            badges.AddRange(metadata.HdrFormats);
        }

        if (enableCodec && metadata.VideoCodec != null)
        {
            badges.Add(NormalizeCodecName(metadata.VideoCodec));
        }

        if (enableSource)
        {
            if (metadata.SourcePrefix != null)
            {
                badges.Add(metadata.SourcePrefix);
            }
            if (metadata.SourceType != null)
            {
                badges.Add(metadata.SourceType);
            }
        }

        if (enableAudio)
        {
            if (metadata.AudioCodecs.Count > 0)
            {
                // Combine audio codec and channels if available
                var audioText = string.Join(" ", metadata.AudioCodecs);
                if (metadata.AudioChannels != null)
                {
                    audioText += $" {metadata.AudioChannels}";
                }
                badges.Add(audioText);
            }
            else if (metadata.AudioChannels != null)
            {
                badges.Add(metadata.AudioChannels);
            }
        }

        // Render badges
        if (badges.Count > 0)
        {
            RenderBadgeStack(canvas, badges, position, originalImage.Width, originalImage.Height, badgeSize);
        }

        return result;
    }

    private void RenderBadgeStack(SKCanvas canvas, List<string> badges, string position, int imageWidth, int imageHeight, int sizePercentage)
    {
        const int baseBadgeHeight = 40;
        const int basePadding = 10;
        const int baseSpacing = 8;

        // Scale based on percentage
        float scale = sizePercentage / 100f;
        int badgeHeight = (int)(baseBadgeHeight * scale);
        int padding = (int)(basePadding * scale);
        int spacing = (int)(baseSpacing * scale);

        // Calculate starting position based on position setting
        int x = padding;
        int y = padding;

        switch (position.ToLowerInvariant())
        {
            case "topright":
                x = imageWidth - padding;
                y = padding;
                break;
            case "bottomleft":
                x = padding;
                y = imageHeight - padding - (badges.Count * (badgeHeight + spacing));
                break;
            case "bottomright":
                x = imageWidth - padding;
                y = imageHeight - padding - (badges.Count * (badgeHeight + spacing));
                break;
            case "topleft":
            default:
                x = padding;
                y = padding;
                break;
        }

        bool rightAlign = position.ToLowerInvariant().Contains("right");

        // Render each badge
        foreach (var badge in badges)
        {
            RenderBadge(canvas, badge, x, y, badgeHeight, rightAlign);
            y += badgeHeight + spacing;
        }
    }

    private void RenderBadge(SKCanvas canvas, string text, int x, int y, int height, bool rightAlign)
    {
        // Calculate badge dimensions
        float fontSize = height * 0.5f;
        using var paint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            TextSize = fontSize,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        var textBounds = new SKRect();
        paint.MeasureText(text, ref textBounds);

        int badgeWidth = (int)(textBounds.Width + height * 0.8f);
        int badgeX = rightAlign ? x - badgeWidth : x;

        // Draw badge background with rounded corners
        using var bgPaint = new SKPaint
        {
            Color = GetBadgeColor(text),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        var rect = new SKRect(badgeX, y, badgeX + badgeWidth, y + height);
        canvas.DrawRoundRect(rect, height * 0.2f, height * 0.2f, bgPaint);

        // Draw badge border
        using var borderPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(100),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawRoundRect(rect, height * 0.2f, height * 0.2f, borderPaint);

        // Draw text centered
        float textX = badgeX + (badgeWidth - textBounds.Width) / 2;
        float textY = y + (height - textBounds.Height) / 2 - textBounds.Top;
        canvas.DrawText(text, textX, textY, paint);
    }

    private SKColor GetBadgeColor(string badge)
    {
        // Color scheme based on badge type
        return badge.ToUpperInvariant() switch
        {
            // Resolution
            "4K" or "2160P" or "8K" => new SKColor(138, 43, 226), // Purple
            "1080P" => new SKColor(30, 144, 255), // Blue
            "720P" => new SKColor(50, 205, 50), // Green
            "480P" or "360P" => new SKColor(255, 140, 0), // Orange

            // HDR
            "DV" => new SKColor(186, 85, 211), // Orchid
            "HDR10+" => new SKColor(148, 0, 211), // Dark Violet
            "HDR10" or "HDR" => new SKColor(138, 43, 226), // Purple
            "HLG" => new SKColor(153, 50, 204), // Dark Orchid

            // Source
            "REMUX" => new SKColor(220, 20, 60), // Crimson
            var s when s.Contains("BLURAY") || s.Contains("BLU-RAY") => new SKColor(0, 0, 205), // Medium Blue
            var s when s.Contains("WEB") => new SKColor(0, 128, 128), // Teal
            "HDTV" => new SKColor(70, 130, 180), // Steel Blue

            // Streaming Services
            "AMZN" => new SKColor(35, 47, 62), // Amazon dark
            "DSNP" or "DISNEY+" => new SKColor(17, 60, 207), // Disney blue
            "HMAX" => new SKColor(0, 0, 128), // HBO blue
            "NF" or "NETFLIX" => new SKColor(229, 9, 20), // Netflix red
            "HULU" => new SKColor(28, 231, 131), // Hulu green

            // Video Codecs
            var c when c.Contains("HEVC") || c.Contains("265") => new SKColor(255, 69, 0), // Red-Orange
            var c when c.Contains("264") || c.Contains("H264") => new SKColor(255, 140, 0), // Dark Orange
            "AV1" => new SKColor(128, 0, 128), // Purple
            "VP9" => new SKColor(75, 0, 130), // Indigo

            // Audio (default if contains audio keywords)
            var a when a.Contains("TRUEHD") || a.Contains("ATMOS") => new SKColor(184, 134, 11), // Dark Goldenrod
            var a when a.Contains("DTS") => new SKColor(255, 215, 0), // Gold
            var a when a.Contains("AAC") || a.Contains("EAC3") || a.Contains("AC3") => new SKColor(218, 165, 32), // Goldenrod

            // Default
            _ => new SKColor(105, 105, 105) // Dim Gray
        };
    }

    private string NormalizeCodecName(string codec)
    {
        return codec.ToUpperInvariant() switch
        {
            "H264" or "H.264" or "X264" => "H.264",
            "H265" or "H.265" or "X265" or "HEVC" => "HEVC",
            _ => codec.ToUpperInvariant()
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var bitmap in _badgeCache.Values)
        {
            bitmap?.Dispose();
        }
        _badgeCache.Clear();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
