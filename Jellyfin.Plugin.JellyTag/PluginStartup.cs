using MediaBrowser.Controller.Plugins;
using Microsoft.AspNetCore.Builder;
using Jellyfin.Plugin.JellyTag.Middleware;

namespace Jellyfin.Plugin.JellyTag;

/// <summary>
/// Plugin startup configuration to register middleware.
/// </summary>
public class PluginStartup : IPluginStartup
{
    /// <inheritdoc />
    public void Configure(IApplicationBuilder app)
    {
        // Register the image badge middleware
        app.UseMiddleware<ImageBadgeMiddleware>();
    }
}
