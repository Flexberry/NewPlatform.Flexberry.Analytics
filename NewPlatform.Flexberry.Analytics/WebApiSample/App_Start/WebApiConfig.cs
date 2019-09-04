namespace NewPlatform.Flexberry.Analytics.WebApiSample
{
    using NewPlatform.Flexberry.AspNet.WebApi.Cors;
    using System.Web.Http;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Конфигурация и службы веб-API
  
            config.EnableCors(new DynamicCorsPolicyProvider(true));

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