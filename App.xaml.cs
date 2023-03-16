using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VideoTool
{
    public partial class App : Application
    {
        public static string FilePath { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (e.Args != null && e.Args.Length > 0)
                FilePath = e.Args[0];
        }
    }
}
