using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Jellyfin.Plugin.JellyTag.Middleware;

namespace Jellyfin.Plugin.JellyTag;

/// <summary>
/// Startup filter to register JellyTag middleware in the HTTP pipeline.
/// </summary>
public class JellyTagStartupFilter : IStartupFilter
{
    /// <inheritdoc />
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            // Register the image badge middleware
            builder.UseMiddleware<ImageBadgeMiddleware>();

            // Continue with the rest of the pipeline
            next(builder);
        };
    }
}
