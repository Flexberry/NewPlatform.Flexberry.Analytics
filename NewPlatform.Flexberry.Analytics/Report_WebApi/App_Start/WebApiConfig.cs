namespace Report_WebApi
{
    using Abstractions;
    using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
    using Microsoft.Practices.Unity.Configuration;
    using NewPlatform.Flexberry.AspNet.WebApi.Cors;
    using Report_Formatters;
    using ReportManager;
    using System.Net.Http.Extensions.Compression.Core.Compressors;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using Unity;
    using Unity.AspNet.WebApi;

    public static class WebApiConfig
    {
        
        public static void Register(HttpConfiguration config)
        {
            // Конфигурация и службы веб-API
            IUnityContainer container = new UnityContainer();
            container.LoadConfiguration();
            container.RegisterInstance<IReportManager>(new PentahoReportManager());
            container.RegisterInstance<IExcelFormatter>(new ExcelFormatter());

            config.DependencyResolver = new UnityDependencyResolver(container);
            config.EnableCors(new DynamicCorsPolicyProvider(true));
            //config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("multipart/form-data"));
            //config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
            //config.MessageHandlers.Insert(0, new ServerCompressionHandler(256, new GZipCompressor(), new DeflateCompressor()));
            config.Formatters.Insert(0,new BinaryMediaTypeFormatter(true));
                //Add(new BinaryMediaTypeFormatter());
            // Маршруты веб-API
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
