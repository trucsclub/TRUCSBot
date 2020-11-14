using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TRUCSBot
{
    class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            var application = new Application();
            application.ConfigureServices(serviceCollection);
                       
            application.Run(args);
            
            Application.Current.ShutdownCompleted += (s, e) =>
            {
                _quitEvent.Set();
            };

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                application.Shutdown();
            };

            _quitEvent.WaitOne();
        }
    }
}
