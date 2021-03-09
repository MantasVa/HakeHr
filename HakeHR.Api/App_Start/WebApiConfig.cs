using System;
using System.Configuration;
using System.Web.Http;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace HakeHR.Api
{
    /// <summary>
    /// Configuration for web api project
    /// </summary>
    public static class WebApiConfig
    {

        /// <summary>
        /// Default method used to configure web api
        /// </summary>
        /// <param name="config">HttpConfiguration type is used to configure web api behaviour</param>
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            Log.Logger = SetupLogger();
            AutofacInitializer.SetAutofacContainer();


            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultActionRoute",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings
                .Add(new System.Net.Http.Formatting.RequestHeaderMapping("Accept",
                    "text/html",
                    StringComparison.InvariantCultureIgnoreCase,
                    true,
                    "application/json"));

        }

        private static Logger SetupLogger()
        {
            var loggerConfig = new LoggerConfiguration();

            string storageAccountConn = ConfigurationManager.AppSettings["STORAGEACCOUNT_CONNECTIONSTRING"];
            if (storageAccountConn != null)
            {
                loggerConfig.WriteTo.AzureBlobStorage(storageAccountConn, LogEventLevel.Information,
                                                                      ConfigurationManager.AppSettings["BLOBSTORAGE_CONTAINERNAME"],
                                                                      "Logs/{yyyy}/{MM}/{dd}d_logs.txt",
                                                                        null,
                                                                        true,
                                                                        TimeSpan.FromSeconds(15),
                                                                        10,
                                                                        true);
            }

            string instrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
            if (instrumentationKey != null)
            {
                var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.InstrumentationKey = instrumentationKey;

                loggerConfig.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);
            }

            return loggerConfig
                        .Enrich.WithMachineName()
                        .Enrich.WithProcessId()
                        .Enrich.FromLogContext()
                        .CreateLogger();
        }
    }
}
