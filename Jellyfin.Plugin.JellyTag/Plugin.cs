using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.JellyTag.Configuration;

namespace Jellyfin.Plugin.JellyTag;

/// <summary>
/// JellyTag Plugin - Automatically overlays quality badges on media posters and thumbnails.
/// Modified to support .strm files by parsing filenames for metadata.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => "JellyTag";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("a8f9f3f1-4b5e-4c3d-9f2e-1a2b3c4d5e6f");

    /// <inheritdoc />
    public override string Description => "Automatically overlays quality badges (resolution, HDR, codec, audio, source) on media posters and thumbnails. Parses .strm filenames for metadata.";

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.config.html"
            }
        };
    }
}
