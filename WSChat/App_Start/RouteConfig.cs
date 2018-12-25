using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace WebApplication1 {
	public static class RouteConfig {
		public static void RegisterRoutes(RouteCollection routes) {
			var settings = new FriendlyUrlSettings();
			settings.AutoRedirectMode = RedirectMode.Permanent;
			routes.EnableFriendlyUrls(settings);

			var url = "WSChat/{*WSHandler}";

			var route = new Route(url, null, null, null, new RouteHandler());

			routes.Add(route);


		}

		public class RouteHandler : IRouteHandler {


			public RouteHandler() {

			}

			public IHttpHandler GetHttpHandler(RequestContext requestContext) {
				
				return null;

			}
		}
	}
}
