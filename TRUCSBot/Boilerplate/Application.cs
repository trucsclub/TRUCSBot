using System;
using System.Reflection;

namespace TRUCSBot
{
    public partial class Application
    {
        public Application()
        {
            if (Current != null)
            {
                throw new Exception("Cannot create a second Application class!");
            }

            Current = this;
        }

        public static bool IsShuttingDown { get; private set; }

        public static Application Current { get; private set; }

        public static string Directory =>
            System.IO.Directory.GetParent(Assembly.GetEntryAssembly()!.Location)!.FullName;

        public event EventHandler ShutdownCompleted;

        public void Run(string[] args)
        {
            OnStartup(args);
        }

        public void Shutdown()
        {
            IsShuttingDown = true;
            OnShutdown();
            ShutdownCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
