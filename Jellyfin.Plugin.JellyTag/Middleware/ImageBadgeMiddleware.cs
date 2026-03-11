using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.JellyTag.Services;

namespace Jellyfin.Plugin.JellyTag.Middleware;

/// <summary>
/// HTTP middleware for intercepting image requests and applying badges.
/// </summary>
public class ImageBadgeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ImageBadgeMiddleware> _logger;
    private readonly ILibraryManager _libraryManager;
    private ImageProcessingService? _imageProcessingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBadgeMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next request delegate.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="libraryManager">The library manager.</param>
    public ImageBadgeMiddleware(
        RequestDelegate next,
        ILogger<ImageBadgeMiddleware> logger,
        ILibraryManager libraryManager)
    {
        _next = next;
        _logger = logger;
        _libraryManager = libraryManager;
    }

    /// <summary>
    /// Processes HTTP requests and intercepts image requests to apply badges.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Check if this is an image request (Primary images for movies/shows)
        if (ShouldProcessRequest(path))
        {
            await ProcessImageRequest(context, path);
            return;
        }

        // Pass through to next middleware
        await _next(context);
    }

    private bool ShouldProcessRequest(string path)
    {
        // Check if plugin is enabled
        var config = Plugin.Instance?.Configuration;
        if (config == null || !config.EnableBadges)
        {
            return false;
        }

        // Only process Primary image requests
        return path.Contains("/Items/") &&
               path.Contains("/Images/Primary") &&
               !path.Contains("/Backdrop");
    }

    private async Task ProcessImageRequest(HttpContext context, string path)
    {
        try
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                await _next(context);
                return;
            }

            // Extract item ID from path
            // Path format: /Items/{itemId}/Images/Primary
            var itemId = ExtractItemIdFromPath(path);
            if (itemId == Guid.Empty)
            {
                await _next(context);
                return;
            }

            // Get the item
            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                await _next(context);
                return;
            }

            // Check if library is excluded
            var metadataService = new MetadataService(_libraryManager);
            if (!metadataService.ShouldApplyBadges(item, config.ExcludedLibraries))
            {
                await _next(context);
                return;
            }

            // Capture the original response
            var originalBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // Let the original request complete
            await _next(context);

            // Only process successful image responses
            if (context.Response.StatusCode == 200 &&
                context.Response.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Read the original image data
                memoryStream.Position = 0;
                var originalImageData = memoryStream.ToArray();

                // Initialize service if needed
                _imageProcessingService ??= new ImageProcessingService(_libraryManager);

                // Process the image with badges
                var processedImageData = _imageProcessingService.ProcessImage(
                    originalImageData,
                    itemId,
                    config.BadgePosition,
                    config.BadgeSize,
                    config.EnableResolutionBadge,
                    config.EnableHdrBadge,
                    config.EnableCodecBadge,
                    config.EnableAudioBadge,
                    config.EnableSourceBadge);

                // Write the processed image
                context.Response.Body = originalBody;
                context.Response.ContentLength = processedImageData.Length;
                context.Response.ContentType = "image/png";
                await context.Response.Body.WriteAsync(processedImageData);
            }
            else
            {
                // Not an image or failed request, return original response
                context.Response.Body = originalBody;
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image request: {Path}", path);
            await _next(context);
        }
    }

    private Guid ExtractItemIdFromPath(string path)
    {
        try
        {
            // Path format: /Items/{itemId}/Images/Primary
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var itemsIndex = Array.FindIndex(segments, s => s.Equals("Items", StringComparison.OrdinalIgnoreCase));

            if (itemsIndex >= 0 && itemsIndex + 1 < segments.Length)
            {
                var itemIdString = segments[itemsIndex + 1];
                if (Guid.TryParse(itemIdString, out var itemId))
                {
                    return itemId;
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return Guid.Empty;
    }
}
