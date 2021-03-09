using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApiSelfHostTest
{
    public class ProxyHandler : DelegatingHandler
    {
        public static List<String> listProxyIPs = new List<string>();
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            string msg = String.Empty;
            //string ip = DAF_Service.GetClientIp(request);
            HttpResponseMessage response;

            //pokud to dojde sem a je to API, tak je neco spatne!
            if (request.RequestUri.AbsolutePath.StartsWith("/api"))
                throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.InternalServerError, String.Format("Není nadefinován žádný Controller s uvedenou cestou [{0} {1}]", request.Method.ToString(), request.RequestUri)));

            //if (DAF_Service.cfg != null && !DAF_Service.cfg.IsEmpty && !String.IsNullOrEmpty(DAF_Service.cfg.FrontEndUrl))
            //{
            //    UriBuilder forwardUri = new UriBuilder(request.RequestUri);

            //    //Extract ewms or mwms from url
            //    string wms_fe_type = String.Empty;
            //    if (request.RequestUri.Host.StartsWith("app"))
            //        wms_fe_type = "app.";
            //    else if (request.RequestUri.Host.StartsWith("mobile"))
            //        wms_fe_type = "mobile.";
            //    else
            //        throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.InternalServerError, String.Format("ProxyHandler definition error: FrontEndUrl [{0}] and ClientUrl [{1}] contains no app/mobile prefix!", DAF_Service.cfg.FrontEndUrl, request.RequestUri)));

            //    //else if (request.RequestUri.AbsolutePath.StartsWith("/ewms"))
            //    //{
            //    //    wms_fe_type = "ewms.";
            //    //    forwardUri.Path = forwardUri.Path.Replace("/ewms", "");
            //    //}
            //    //else if (request.RequestUri.AbsolutePath.StartsWith("/mwms"))
            //    //{
            //    //    wms_fe_type = "mwms.";
            //    //    forwardUri.Path = forwardUri.Path.Replace("/mwms", "");
            //    //}

            //    if (wms_fe_type.Length > 0 && (DAF_Service.cfg.FrontEndUrl.StartsWith("ewms") || DAF_Service.cfg.FrontEndUrl.StartsWith("mwms")))
            //        throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.InternalServerError, String.Format("ProxyHandler definition error: FrontEndUrl [{0}] and ClientUrl [{1}] both contains ewms/mwms identifier!", DAF_Service.cfg.FrontEndUrl, request.RequestUri)));

            //    //strip off the proxy port and replace with an Http port
            //    forwardUri.Port = DAF_Service.cfg.FrontEndPort;
            //    forwardUri.Host = wms_fe_type + DAF_Service.cfg.FrontEndUrl;

            //    //VP: radeji zalozim novy request, at nema zadne headers atd... coz je asi cistsi...?
            //    ////send it on to the requested URL
            //    //request.RequestUri = forwardUri.Uri;
            //    ////have to explicitly null it to avoid protocol violation
            //    //if (request.Method == HttpMethod.Get)
            //    //    request.Content = null;

            //    HttpClient client = new HttpClient();
            //    HttpRequestMessage forwardRequest = new HttpRequestMessage(HttpMethod.Get, forwardUri.Uri);
            //    msg = String.Format("ProxyHandler - ClientIP: {0} url {1} to {2}", ip, request.RequestUri, forwardRequest.RequestUri);
            //    response = await client.SendAsync(forwardRequest, HttpCompletionOption.ResponseHeadersRead);
            //}
            //else
            //{
            //msg = String.Format("ProxyHandler is not set - ClientIP: {0} url {1}. Returning HttpStatusCode.NotFound (404)", ip, request.RequestUri);
            response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            //}

            //if (!listProxyIPs.Contains(ip))
            //{
            //    listProxyIPs.Add(ip);

            //    if (Environment.UserInteractive)
            //        Console.WriteLine(msg);

            //    if (DAF_Service.ConnData.IsValid() && DAF_Service.Settings.Logovat_RestApi)
            //    {
            //        UtilsAF.WriteLog(AF_Log_Type.DetailInfo, DAF_Service.GetDatabase(), msg, request.Headers.ToString());
            //    }
            //}
            return response;
        }
    }
}
