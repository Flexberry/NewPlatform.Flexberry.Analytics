namespace NewPlatform.Flexberry.Analytics.WebAPI
{
    using System.Web.Http;

    public static class ReportApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "ReportApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}