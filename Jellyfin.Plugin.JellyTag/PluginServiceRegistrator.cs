using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Jellyfin.Plugin.JellyTag.Middleware;

namespace Jellyfin.Plugin.JellyTag;

/// <summary>
/// Plugin service registrator for dependency injection.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        // Register middleware
        serviceCollection.AddSingleton<ImageBadgeMiddleware>();
    }
}
