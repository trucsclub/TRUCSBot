using System;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

namespace TRUCSBot
{
    internal class Program
    {
        private static readonly ManualResetEvent QuitEvent = new(false);

        private static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            var application = new Application();
            application.ConfigureServices(serviceCollection);

            application.Run(args);

            Application.Current.ShutdownCompleted += (_, _) => { QuitEvent.Set(); };

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                application.Shutdown();
            };

            QuitEvent.WaitOne();
        }
    }
}
