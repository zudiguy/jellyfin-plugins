using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.JellyTag.Services;

/// <summary>
/// Service for retrieving media metadata from Jellyfin items.
/// Integrates with FileNameParser to extract metadata from .strm filenames.
/// </summary>
public class MetadataService
{
    private readonly ILibraryManager _libraryManager;
    private readonly FileNameParser _fileNameParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataService"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    public MetadataService(ILibraryManager libraryManager)
    {
        _libraryManager = libraryManager;
        _fileNameParser = new FileNameParser();
    }

    /// <summary>
    /// Gets metadata for a media item by its ID.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    /// <returns>Parsed media metadata or null if item not found.</returns>
    public MediaMetadata? GetMetadata(Guid itemId)
    {
        var item = _libraryManager.GetItemById(itemId);
        if (item == null)
        {
            return null;
        }

        return GetMetadataFromItem(item);
    }

    /// <summary>
    /// Gets metadata from a BaseItem.
    /// </summary>
    /// <param name="item">The media item.</param>
    /// <returns>Parsed media metadata.</returns>
    public MediaMetadata GetMetadataFromItem(BaseItem item)
    {
        // Try to get the file path
        var path = item.Path;

        if (string.IsNullOrEmpty(path))
        {
            // Fallback to first media source path if available
            var mediaSources = item.GetMediaSources(false);
            if (mediaSources.Count > 0)
            {
                path = mediaSources[0].Path;
            }
        }

        if (string.IsNullOrEmpty(path))
        {
            return new MediaMetadata();
        }

        // Check if it's a .strm file
        if (path.EndsWith(".strm", StringComparison.OrdinalIgnoreCase))
        {
            // Extract filename without extension
            var filename = Path.GetFileNameWithoutExtension(path);
            return _fileNameParser.Parse(filename);
        }

        // For non-.strm files, could potentially fall back to Jellyfin's metadata
        // For now, return empty metadata
        return new MediaMetadata();
    }

    /// <summary>
    /// Checks if an item should have badges based on configuration.
    /// </summary>
    /// <param name="item">The media item.</param>
    /// <param name="excludedLibraries">List of excluded library IDs.</param>
    /// <returns>True if badges should be applied, false otherwise.</returns>
    public bool ShouldApplyBadges(BaseItem item, string[] excludedLibraries)
    {
        if (item == null)
        {
            return false;
        }

        // Check if item's library is excluded
        var libraryId = item.ParentId.ToString();

        // Try to find the actual library root
        var parent = item.GetParent();
        while (parent != null && parent.GetParent() != null)
        {
            libraryId = parent.Id.ToString();
            parent = parent.GetParent();
        }

        return !excludedLibraries.Contains(libraryId, StringComparer.OrdinalIgnoreCase);
    }
}
