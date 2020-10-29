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
            var application = new Application();
            application.Run(args);

            Application.Current.ShutdownCompleted += (s, e) =>
            {
                Console.WriteLine("Application shutdown complete.");
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
