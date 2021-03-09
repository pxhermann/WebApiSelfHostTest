using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;

[assembly: OwinStartup(typeof(WebApiSelfHostTest.Startup))]
namespace WebApiSelfHostTest
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            // enable global level CORS
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

        // routing
            // enable attribute routing ~ WebAPI 2
            config.MapHttpAttributeRoutes();
            // convention-based routing ~ WebAPI 1
            config.Routes.MapHttpRoute( 
                name: "DefaultApi", 
                routeTemplate: "api/{controller}/{id}", 
                defaults: new { id = RouteParameter.Optional } 
            );
            config.Routes.MapHttpRoute(
                name: "Proxy", 
                routeTemplate: "{*path}", 
                handler: HttpClientFactory.CreatePipeline(new HttpClientHandler(),new DelegatingHandler[] { new ProxyHandler() }),  // will never get here if proxy is doing its job
                defaults: new { path = RouteParameter.Optional },
                constraints: null
            );

        // media formatters
            // default JSON formatter customization
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                Formatting = Newtonsoft.Json.Formatting.Indented,   // write intended JSON
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(), // write JSON property names with camel casing
            };

        // custom filters
            config.Filters.Add(new CustomActionFilterAttribute());
            string authKey = AppSetting.Load().AuthorizationKey;
            if ( !string.IsNullOrEmpty(authKey) )
                config.Filters.Add(new CustomAuthorizationFilterAttribute(authKey));

        // error handling
            // exception filter
            config.Filters.Add(new CustomExceptionFilterAttribute());
            // global exception handler
            config.Services.Replace(typeof(IExceptionHandler), new CustomExceptionHandler());


         // logging - ukladani Response/Request do logu...
            config.MessageHandlers.Add(new ContentInterceptorHandler());

            app.UseWebApi(config); 
/*            app.Use(async (ctx, next) =>  
            {
                await ctx.Response.WriteAsync("Hallo from selft hosted api");
            });
*/
        }


        public class ContentInterceptorHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request.Content != null && request.Content.Headers != null && request.Content.Headers.ContentType != null &&
                    request.Content.Headers.ContentLength < 4096)   // loguj jen zpravy kratsi nez 4kB
                {
                    var requestBody = await request.Content.ReadAsStringAsync();
                    request.Properties["SavedBodyContent"] = requestBody;
                    request.Content = new StringContent(requestBody, Encoding.UTF8, request.Content.Headers.ContentType.MediaType);
                }
                return await base.SendAsync(request, cancellationToken);
            }
        }

    }
}
