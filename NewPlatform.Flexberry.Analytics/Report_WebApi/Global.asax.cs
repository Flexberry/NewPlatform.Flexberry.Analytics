using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.Web.Http;

namespace Report_WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            
            ZipConstants.DefaultCodePage = Encoding.Default.CodePage;
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(config => {
                //  config.MapHttpAttributeRoutes();
                config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
                config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                //config.AddJsonpFormatter();
                //var customODataSerializerProvider = new CustomODataSerializerProvider();
                //var extendedODataDeserializerProvider = new ExtendedODataDeserializerProvider();
                //var odataFormatters = ODataMediaTypeFormatters.Create(customODataSerializerProvider, extendedODataDeserializerProvider);
                //config.Formatters.InsertRange(0, odataFormatters);
                //config.Properties[typeof(CustomODataSerializerProvider)] = customODataSerializerProvider;
            });

        }
    }
}
