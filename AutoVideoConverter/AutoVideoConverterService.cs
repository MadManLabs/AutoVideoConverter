using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AutoVideoConverter
{
    public partial class AutoVideoConverterService : ServiceBase
    {
        public AutoVideoConverterService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            FileSystemWatcher fsw  = new FileSystemWatcher()
            {
                EnableRaisingEvents = true
            };
            
            

        }

        protected override void OnStop()
        {
        }
    }
}
