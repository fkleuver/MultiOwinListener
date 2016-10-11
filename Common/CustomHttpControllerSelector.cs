using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Http.Dispatcher;
using System.Web.Http;
using Common.Logging;

namespace Common
{
    public sealed class CustomHttpControllerSelector : DefaultHttpControllerSelector
    {
        private static readonly ILog Logger;

        static CustomHttpControllerSelector()
        {
            Logger = LogProvider.GetCurrentClassLogger();
        }

        public CustomHttpControllerSelector(HttpConfiguration configuration) : base(configuration)
        {
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var apiControllerAssembly = request.GetOwinEnvironment()["ApiControllersAssembly"].ToString();
            Logger.Debug($"{nameof(CustomHttpControllerSelector)}: {{{nameof(apiControllerAssembly)}: {apiControllerAssembly}}}");

            var routeData = request.GetRouteData();
            var routeCollectionRoute = routeData.Route as IReadOnlyCollection<IHttpRoute>;
            var newRoutes = new List<IHttpRoute>();
            var newRouteCollectionRoute = new RouteCollectionRoute();
            foreach (var route in routeCollectionRoute)
            {
                var filteredDataTokens = FilterDataTokens(route, apiControllerAssembly);
                if (filteredDataTokens.Count == 2)
                {
                    var newRoute = new HttpRoute(route.RouteTemplate, (HttpRouteValueDictionary)route.Defaults, (HttpRouteValueDictionary)route.Constraints, filteredDataTokens);
                    newRoutes.Add(newRoute);
                }
            }

            var newRouteDataValues = new HttpRouteValueDictionary();
            foreach (var routeDataKvp in routeData.Values)
            {
                var newRouteDataCollection = new List<IHttpRouteData>();
                var routeDataCollection = routeDataKvp.Value as IEnumerable<IHttpRouteData>;
                if (routeDataCollection != null)
                {
                    foreach (var innerRouteData in routeDataCollection)
                    {
                        var filteredDataTokens = FilterDataTokens(innerRouteData.Route, apiControllerAssembly);
                        if (filteredDataTokens.Count == 2)
                        {
                            var newInnerRoute = new HttpRoute(innerRouteData.Route.RouteTemplate, (HttpRouteValueDictionary)innerRouteData.Route.Defaults, (HttpRouteValueDictionary)innerRouteData.Route.Constraints, filteredDataTokens);
                            var newInnerRouteData = new HttpRouteData(newInnerRoute, (HttpRouteValueDictionary)innerRouteData.Values);
                            newRouteDataCollection.Add(newInnerRouteData);
                        }
                    }
                    newRouteDataValues.Add(routeDataKvp.Key, newRouteDataCollection);
                }
                else
                {
                    newRouteDataValues.Add(routeDataKvp.Key, routeDataKvp.Value);
                }

                HttpRouteData newRouteData;
                if (newRoutes.Count > 1)
                {
                    newRouteCollectionRoute.EnsureInitialized(() => newRoutes);
                    newRouteData = new HttpRouteData(newRouteCollectionRoute, newRouteDataValues);
                }
                else
                {
                    newRouteData = new HttpRouteData(newRoutes[0], newRouteDataValues);
                }
                request.SetRouteData(newRouteData);
            }

           
            var controllerDescriptor = base.SelectController(request);
            return controllerDescriptor;
        }

        private static HttpRouteValueDictionary FilterDataTokens(IHttpRoute route, string apiControllerAssembly)
        {
            var newDataTokens = new HttpRouteValueDictionary();
            foreach (var dataToken in route.DataTokens)
            {
                var actionDescriptors = dataToken.Value as IEnumerable<HttpActionDescriptor>;
                if (actionDescriptors != null)
                {
                    var newActionDescriptors = new List<HttpActionDescriptor>();
                    foreach (var actionDescriptor in actionDescriptors)
                    {
                        if (actionDescriptor.ControllerDescriptor.ControllerType.Assembly.FullName == apiControllerAssembly)
                        {
                            newActionDescriptors.Add(actionDescriptor);
                        }
                    }
                    if (newActionDescriptors.Count > 0)
                    {
                        newDataTokens.Add(dataToken.Key, newActionDescriptors.ToArray());
                    }
                }
                else
                {
                    newDataTokens.Add(dataToken.Key, dataToken.Value);
                }
            }
            return newDataTokens;
        }
    }
}
