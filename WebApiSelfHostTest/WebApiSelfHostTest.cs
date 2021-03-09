using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebApiSelfHostTest
{
    public partial class WebApiSelfHostTest : ServiceBase
    {
        private Thread thread = null;
        private bool threadRunning = false;
        private IDisposable webApp = null;

        public WebApiSelfHostTest()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry(ServiceName + " starting...");

            try
            {
                thread = new Thread(ServiceThreadProc);
                thread.Start();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(String.Format("Error occured when starting {0}: {1}", ServiceName, ex.ToString()));
                ExitCode = 1003; //Cannot complete this function.
                Stop();
                return;
            }

            EventLog.WriteEntry(ServiceName + " started");
        }

        protected override void OnStop()
        {
            threadRunning = false;

            if ( webApp!= null )
                webApp.Dispose();

            if (thread != null)
            {
                thread.Join(3000); // wait max. 3 seconds to finish thread

                if ( thread.IsAlive ) // if thread is still alive (not finished regularly), kill it
                    thread.Abort();
            }

            EventLog.WriteEntry(ServiceName + " stopped");
        }
        private void ServiceThreadProc()
        {
            threadRunning = true;

            webApp = WebApp.Start<Startup>((new AppSetting()).UrlBase);
            while (threadRunning)
                Thread.Sleep(200);
        }
   }
}
