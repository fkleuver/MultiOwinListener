using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using Common;
using Common.Logging;
using LibA.Controllers;
using LibB.Controllers;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using Serilog;
using Serilog.Events;

namespace Host
{
    internal class Program
    {
        private static readonly ILog Logger;

        static Program()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .LiterateConsole()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .CreateLogger();

            Logger = LogProvider.GetCurrentClassLogger();
        }

        internal static void Main(string[] args)
        {

            var builder = new ContainerBuilder();
            builder.RegisterModule(new LogRequestModule());
            builder.RegisterApiControllers(typeof(AController).Assembly);
            builder.RegisterApiControllers(typeof(BController).Assembly);

            var container = builder.Build();

            var config = GetHttpConfig();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            var options = new StartOptions();
            options.Urls.Add("http://localhost:1234");
            options.Urls.Add("http://localhost:5678");

            var listener = WebApp.Start(options, app =>
            {
                app.Use((ctx, next) =>
                {
                    if (ctx.Request.LocalPort.HasValue)
                    {
                        var port = ctx.Request.LocalPort.Value;
                        string apiControllersAssemblyName = null;
                        if (port == 1234)
                        {
                            apiControllersAssemblyName = typeof(AController).Assembly.FullName;
                        }
                        else if (port == 5678)
                        {
                            apiControllersAssemblyName = typeof(BController).Assembly.FullName;
                        }
                        ctx.Set("ApiControllersAssembly", apiControllersAssemblyName);
                        Logger.Info($"{nameof(WebApp)}: Port = {port}, ApiControllersAssembly = {apiControllersAssemblyName}");
                    }
                    return next();
                });
                app.UseAutofacMiddleware(container);
                app.UseAutofacWebApi(config);
                app.UseWebApi(config);
            });


            Logger.Info(@"Press [Enter] to exit");

            Console.ReadLine();

            listener.Dispose(); ;
        }


        private static HttpConfiguration GetHttpConfig()
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.Services.Add(typeof(IExceptionLogger), new LogProviderExceptionLogger());
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Services.Replace(typeof(IHttpControllerSelector), new CustomHttpControllerSelector(config));
            config.Services.Replace(typeof(IHttpActionSelector), new CustomHttpActionSelector());

            var traceSource = new TraceSource("LibLog") { Switch = { Level = SourceLevels.All } };
            traceSource.Listeners.Add(new LibLogTraceListener());

            var diag = config.EnableSystemDiagnosticsTracing();
            diag.IsVerbose = false;
            diag.TraceSource = traceSource;

            return config;
        }
    }
}
