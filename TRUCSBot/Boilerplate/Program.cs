using System;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

namespace TRUCSBot
{
    internal class Program
    {
        private static readonly ManualResetEvent _quitEvent = new(false);

        private static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            var application = new Application();
            application.ConfigureServices(serviceCollection);

            application.Run(args);

            Application.Current.ShutdownCompleted += (s, e) => { _quitEvent.Set(); };

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                application.Shutdown();
            };

            _quitEvent.WaitOne();
        }
    }
}
