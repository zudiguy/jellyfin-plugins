using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using MediaBrowser.Controller.Library;
using SkiaSharp;

namespace Jellyfin.Plugin.JellyTag.Services;

/// <summary>
/// Service for processing images with badge overlays and caching.
/// </summary>
public class ImageProcessingService : IDisposable
{
    private readonly ILibraryManager _libraryManager;
    private readonly MetadataService _metadataService;
    private readonly BadgeRenderer _badgeRenderer;
    private readonly ConcurrentDictionary<string, byte[]> _cache;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageProcessingService"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    public ImageProcessingService(ILibraryManager libraryManager)
    {
        _libraryManager = libraryManager;
        _metadataService = new MetadataService(libraryManager);
        _badgeRenderer = new BadgeRenderer();
        _cache = new ConcurrentDictionary<string, byte[]>();
    }

    /// <summary>
    /// Processes an image by adding badges based on item metadata.
    /// </summary>
    /// <param name="originalImageData">The original image data.</param>
    /// <param name="itemId">The media item ID.</param>
    /// <param name="position">Badge position.</param>
    /// <param name="badgeSize">Badge size percentage.</param>
    /// <param name="enableResolution">Enable resolution badges.</param>
    /// <param name="enableHdr">Enable HDR badges.</param>
    /// <param name="enableCodec">Enable codec badges.</param>
    /// <param name="enableAudio">Enable audio badges.</param>
    /// <param name="enableSource">Enable source badges.</param>
    /// <returns>Processed image data with badges, or original if processing fails.</returns>
    public byte[] ProcessImage(
        byte[] originalImageData,
        Guid itemId,
        string position = "TopRight",
        int badgeSize = 100,
        bool enableResolution = true,
        bool enableHdr = true,
        bool enableCodec = true,
        bool enableAudio = true,
        bool enableSource = true)
    {
        try
        {
            // Generate cache key
            var cacheKey = GenerateCacheKey(itemId, position, badgeSize, enableResolution, enableHdr, enableCodec, enableAudio, enableSource);

            // Check cache
            if (_cache.TryGetValue(cacheKey, out var cachedImage))
            {
                return cachedImage;
            }

            // Get metadata for the item
            var metadata = _metadataService.GetMetadata(itemId);
            if (metadata == null || !metadata.HasMetadata())
            {
                // No metadata found, return original image
                return originalImageData;
            }

            // Load the original image
            using var originalBitmap = SKBitmap.Decode(originalImageData);
            if (originalBitmap == null)
            {
                return originalImageData;
            }

            // Render badges
            using var processedBitmap = _badgeRenderer.RenderBadges(
                originalBitmap,
                metadata,
                position,
                badgeSize,
                enableResolution,
                enableHdr,
                enableCodec,
                enableAudio,
                enableSource);

            // Encode to bytes
            using var image = SKImage.FromBitmap(processedBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 95);
            var processedData = data.ToArray();

            // Cache the result
            _cache.TryAdd(cacheKey, processedData);

            return processedData;
        }
        catch (Exception)
        {
            // If anything goes wrong, return the original image
            return originalImageData;
        }
    }

    /// <summary>
    /// Clears the image cache.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Gets the current cache size.
    /// </summary>
    /// <returns>Number of cached images.</returns>
    public int GetCacheSize()
    {
        return _cache.Count;
    }

    private string GenerateCacheKey(
        Guid itemId,
        string position,
        int badgeSize,
        bool enableResolution,
        bool enableHdr,
        bool enableCodec,
        bool enableAudio,
        bool enableSource)
    {
        var key = $"{itemId}_{position}_{badgeSize}_{enableResolution}_{enableHdr}_{enableCodec}_{enableAudio}_{enableSource}";

        // Generate a hash to keep cache keys short
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        return Convert.ToBase64String(hashBytes);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _badgeRenderer?.Dispose();
        _cache?.Clear();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
