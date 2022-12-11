using log4net;
using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

namespace DynDnsClient.Service
{
    static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Log.Info("Starting service");


            var service = new Service();

            if (Debugger.IsAttached)
            {
                var serviceType = typeof(Service);
                try
                {
                    // OnStart is protected so need to be creative to call it
                    var onStart = serviceType.GetMethod("OnStart", BindingFlags.NonPublic | BindingFlags.Instance);
                    onStart.Invoke(service, new object[] { null });

                    Console.WriteLine("Press any key to stop the program.");
                    Console.Read();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    var onStop = serviceType.GetMethod("OnStop", BindingFlags.NonPublic | BindingFlags.Instance);
                    onStop.Invoke(service, parameters: null);
                }
            }
            else
            {
                ServiceBase.Run(new[] { service });
            }
        }
    }
}