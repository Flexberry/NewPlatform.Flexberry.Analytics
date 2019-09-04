using NewPlatform.Flexberry.Analytics.WebAPI;
using System.Web.Http;

namespace NewPlatform.Flexberry.Analytics.WebApiSample
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(ReportApiConfig.Register);
        }
    }
}