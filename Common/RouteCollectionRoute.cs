using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Routing;

namespace Common
{
    public class RouteCollectionRoute : IHttpRoute, IReadOnlyCollection<IHttpRoute>
    {
        public const string SubRouteDataKey = "MS_SubRoutes";

        private IReadOnlyCollection<IHttpRoute> _subRoutes;

        private static readonly IDictionary<string, object> _empty = new Dictionary<string, object>();

        public RouteCollectionRoute()
        {
        }
        
        private bool _beingInitialized;
        
        public void EnsureInitialized(Func<IReadOnlyCollection<IHttpRoute>> initializer)
        {
            if (_beingInitialized && _subRoutes == null)
            {
                return;
            }

            try
            {
                _beingInitialized = true;

                _subRoutes = initializer();
                Contract.Assert(_subRoutes != null);
            }
            finally
            {
                _beingInitialized = false;
            }
        }

        private IReadOnlyCollection<IHttpRoute> SubRoutes => _subRoutes;

        public string RouteTemplate => String.Empty;

        public IDictionary<string, object> Defaults => _empty;

        public IDictionary<string, object> Constraints => _empty;

        public IDictionary<string, object> DataTokens => null;

        public HttpMessageHandler Handler => null;

        public IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
        {
            List<IHttpRouteData> matches = new List<IHttpRouteData>();
            foreach (IHttpRoute route in SubRoutes)
            {
                IHttpRouteData match = route.GetRouteData(virtualPathRoot, request);
                if (match != null)
                {
                    matches.Add(match);
                }
            }
            if (matches.Count == 0)
            {
                return null;  // no matches
            }

            return new RouteCollectionRouteData(this, matches.ToArray());
        }

        public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            return null;
        }

        public int Count => SubRoutes.Count;

        public IEnumerator<IHttpRoute> GetEnumerator()
        {
            return SubRoutes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SubRoutes.GetEnumerator();
        }
        
        private class RouteCollectionRouteData : IHttpRouteData
        {
            public RouteCollectionRouteData(IHttpRoute parent, IHttpRouteData[] subRouteDatas)
            {
                Route = parent;
                
                Values = new HttpRouteValueDictionary() { { SubRouteDataKey, subRouteDatas } };
            }

            public IHttpRoute Route { get; private set; }

            public IDictionary<string, object> Values { get; private set; }
        }
    }
}
