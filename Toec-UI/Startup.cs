using System.Web.Http;
using Newtonsoft.Json;
using Owin;

namespace Toec_UI
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("DefaultApi", "ToecUI/{controller}/{action}/{id}",
                new {id = RouteParameter.Optional}
                );

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;

            appBuilder.UseWebApi(config);
        }
    }
}