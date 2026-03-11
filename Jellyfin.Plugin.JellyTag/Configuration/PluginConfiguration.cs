using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.JellyTag.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        EnableBadges = true;
        BadgePosition = "TopRight";
        BadgeSize = 100;
        EnableResolutionBadge = true;
        EnableHdrBadge = true;
        EnableCodecBadge = true;
        EnableAudioBadge = true;
        EnableSourceBadge = true;
        ExcludedLibraries = Array.Empty<string>();
    }

    /// <summary>
    /// Gets or sets a value indicating whether badges are enabled.
    /// </summary>
    public bool EnableBadges { get; set; }

    /// <summary>
    /// Gets or sets the badge position (TopLeft, TopRight, BottomLeft, BottomRight).
    /// </summary>
    public string BadgePosition { get; set; }

    /// <summary>
    /// Gets or sets the badge size percentage (50-200).
    /// </summary>
    public int BadgeSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether resolution badges are enabled.
    /// </summary>
    public bool EnableResolutionBadge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether HDR badges are enabled.
    /// </summary>
    public bool EnableHdrBadge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether codec badges are enabled.
    /// </summary>
    public bool EnableCodecBadge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether audio badges are enabled.
    /// </summary>
    public bool EnableAudioBadge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether source badges are enabled.
    /// </summary>
    public bool EnableSourceBadge { get; set; }

    /// <summary>
    /// Gets or sets the list of excluded library IDs.
    /// </summary>
    public string[] ExcludedLibraries { get; set; }
}
