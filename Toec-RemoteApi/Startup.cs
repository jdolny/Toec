using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Owin;

namespace Toec_RemoteApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.Properties.Add("Remote", true);
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("DefaultApi", "toec/{controller}/{action}/{id}",
                new {id = RouteParameter.Optional}
                );
            if (appBuilder.Properties.ContainsKey("Remote"))
            {
                var ar = new IntegrationTestAssembliesResolver();
                config.Services.Replace(typeof (IAssembliesResolver), ar);
            }
            appBuilder.UseWebApi(config);
        }

        public class IntegrationTestAssembliesResolver : IAssembliesResolver
        {
            public ICollection<Assembly> GetAssemblies()
            {
                return new[] {this.GetType().Assembly};
            }
        }
    }
}