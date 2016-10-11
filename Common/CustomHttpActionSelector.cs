using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Common.Logging;

namespace Common
{
    public sealed class CustomHttpActionSelector : ApiControllerActionSelector
    {
        private static readonly ILog Logger;

        static CustomHttpActionSelector()
        {
            Logger = LogProvider.GetCurrentClassLogger();
        }

        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            try
            {
                var actionDescriptor = base.SelectAction(controllerContext);
                return actionDescriptor;
            }
            catch (Exception ex)
            {
                Logger.WarnException(ex.Message, ex);

                IDictionary<string, object> dataTokens;
                var route = controllerContext.Request.GetRouteData().Route;
                var routeCollectionRoute = route as IReadOnlyCollection<IHttpRoute>;
                if (routeCollectionRoute != null)
                {
                    dataTokens = routeCollectionRoute
                        .Select(r => r.DataTokens)
                        .SelectMany(dt => dt)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                else
                {
                    dataTokens = route.DataTokens;
                }

                var actionDescriptors = dataTokens
                    .Select(dt => dt.Value)
                    .Where(dt => dt is IEnumerable<HttpActionDescriptor>)
                    .Cast<IEnumerable<HttpActionDescriptor>>()
                    .SelectMany(r => r)
                    .ToList();

                return actionDescriptors.FirstOrDefault();
            }

        }
    }
}
