using System.Web.Http;

namespace NewPlatform.Flexberry.Analytics.WebAPI
{
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