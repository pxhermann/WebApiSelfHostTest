using Microsoft.Owin.Hosting;
using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace WebApiSelfHostTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if ( !Environment.UserInteractive )
            {   // run as windows service = as program was intended and build

/*              ServiceBase[] ServicesToRun = new ServiceBase[] { new TestService() };
                ServiceBase.Run(ServicesToRun);
*/
                ServiceBase.Run(new WebApiSelfHostTest());
            }
            else // programatically switch to console application
            {
                try
                {
                    AllocConsole();
                    
                    Console.WriteLine("Starting");

                    string uri = (new AppSetting()).UrlBase;
                    using ( WebApp.Start<Startup>(uri) )
                    {
                        Console.WriteLine(String.Format("WebApiSelfHostTest started on {0}. Press any key to exit...", uri));
                        Console.ReadKey();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine("Error occured: " + GM.FormatException(ex));
                }
            }
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}
