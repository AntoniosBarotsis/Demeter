using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Application.Config
{
    /// <summary>
    ///     Adds a route prefix to the controllers
    /// </summary>
    public static class MvcOptionsExtensions
    {
        private static void UseGeneralRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
        {
            opts.Conventions.Add(new RoutePrefixConvention(routeAttribute));
        }

        /// <summary>
        ///     Adds the route attribute
        /// </summary>
        /// <param name="opts"></param>
        /// <param name="prefix"></param>
        public static void UseGeneralRoutePrefix(this MvcOptions opts, string prefix)
        {
            opts.UseGeneralRoutePrefix(new RouteAttribute(prefix));
        }
    }
}