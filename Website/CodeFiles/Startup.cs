using Microsoft.AspNet.SignalR;
using Owin;

[assembly: Microsoft.Owin.OwinStartup(typeof(GalleryServer.Web.Startup))]

namespace GalleryServer.Web
{
  /// <summary>
  /// Contains functionality for wiring up OWIN functionality during application startup.
  /// </summary>
  public class Startup
	{
    /// <summary>
    /// Configures the current application for OWIN-related functionality.
    /// </summary>
    /// <param name="app">The current application.</param>
    public void Configuration(IAppBuilder app)
		{
			// Any connection or hub wire up and configuration should go here
      app.MapSignalR(@"/gs/signalr", new HubConfiguration { EnableDetailedErrors = Utils.IsDebugEnabled });
		}
	}
}