using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
namespace WebApiSelfHostTest
{
    public class CustomAuthorizationFilterAttribute : AuthorizationFilterAttribute
    {
        private string authorizationKey = null;
        public CustomAuthorizationFilterAttribute(string authKey)
		{
            authorizationKey = authKey;
        }
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                if (actionContext.Request.RequestUri.AbsolutePath.StartsWith("/api/public"))
                    return;

                string key = "";
                if (actionContext.Request.Headers.Authorization != null)
                {
                    string p = actionContext.Request.Headers.Authorization.Parameter;
                    if ( !string.IsNullOrEmpty(p) )
                        key = Encoding.ASCII.GetString(Convert.FromBase64String(p));
                }
                else if (actionContext.Request.RequestUri.Query.Contains("X-SingleAuth"))
                {
                    NameValueCollection param_list = HttpUtility.ParseQueryString(actionContext.Request.RequestUri.Query);
                    key = param_list["X-SingleAuth"];
                }
                else if (actionContext.Request.Headers.Contains("X-Token"))
                    key = actionContext.Request.Headers.GetValues("X-Token").First();

                if ( !string.IsNullOrEmpty(key) && key == authorizationKey )
                    return;

                string strMsg = string.IsNullOrEmpty(key) ? "No authorization key" : "Access denied : wrong authorization key";
                //not authorized
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, strMsg);
                GM.WriteLog(LogType.Error, strMsg);
            }
            catch (Exception ex)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Authorization failed: " + ex.Message);
                GM.WriteLog(LogType.Error, "Authorization failed");
            }
        }
    }

    public class CustomActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            string url = HttpUtility.UrlDecode(actionContext.Request.RequestUri.LocalPath).ToLower();
            if (url.StartsWith("/api/user/keepalive") || url.StartsWith("/api/log") || url.StartsWith("/api/public"))
                return;

            if (url.StartsWith("/api/user/login"))
                return;

            GM.WriteLog(LogType.Info, string.Format("Executing {0} on Url: {1}", actionContext.Request.Method, url));
        }
/*		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			base.OnActionExecuted(actionExecutedContext);
		}
*/	}

    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
		public override void OnException(HttpActionExecutedContext context)
		{
            if ( context == null )
            {
                base.OnException(context);
                return;
            }
            // write log
			GM.WriteLog(context.Exception, "Unprocessed exception in URL: " + context.Request.RequestUri);

            // set result
//            context.Response = context.ActionContext.Request.CreateErrorResponse(HttpStatusCode.Conflict, s, context.Exception);
            string s = (context.Exception == null)?"Unknown error occured":context.Exception.Message;
            context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError) {
				Content = new StringContent(s, Encoding.UTF8, "application/text")
			};
        }
    }

    public class CustomExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            if (context == null)
            {
                base.Handle(context);
                return;
            }

            // write log
            GM.WriteLog(context.Exception, "Unhandled exception in URL: " + context.Request.RequestUri);

            // set result
            context.Result = new ExceptionResult(context.Request, context.Exception);
        }
    }

    class ExceptionResult : IHttpActionResult
    {
        private readonly HttpRequestMessage request;
        private readonly Exception exception;

        public ExceptionResult(HttpRequestMessage request, Exception exception)
        {
            this.request = request;
            this.exception = exception;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = request.CreateResponse(HttpStatusCode.PreconditionFailed, (exception == null)?"":exception.Message);
            return Task.FromResult(response);
        }
    }
}
