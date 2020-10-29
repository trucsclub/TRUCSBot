/*
 * 
 * 
 * You probably don't need to be editing this file. You should be editing Application.cs
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TRUCSBot
{
    public partial class Application
    {
        public static Application Current { get; private set; }
        public static bool IsShuttingDown = false;
        public event EventHandler ShutdownCompleted;

        public string Directory => System.IO.Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName;

        public Application()
        {
            if (Current != null) throw new Exception("Cannot create a second Application class!");
            Current = this;
        }

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
