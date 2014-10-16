using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceProcess;


namespace AutoVideoConverter
{
    public partial class AutoVideoConverterService : ServiceBase
    {
        public AutoVideoConverterService()
        {
            InitializeComponent();
        }

        private static void InitializeFileSystemWatcher()
        {
            //Set up new FileSystemWatcher watching for files in path defined in AppSettings[WatchingPath]
            var fsw = new FileSystemWatcher()
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                Path = ConfigurationSettings.AppSettings["WatchingPath"],
                InternalBufferSize = 64000

            };

            fsw.Created += OnChange;

        }

        private static Queue<string> ProcessQueue = new Queue<string>();

        private static bool ProgramIsBusy;

        private static void ConvertVideoFromQueue(string inputPath)
        {
            //Pick up file paths of files that are created in WatchingPath directory then pass them into converter.
            var fileConverter = new NReco.VideoConverter.FFMpegConverter();
            fileConverter.ConvertMedia(inputPath, ConfigurationSettings.AppSettings["OutputPath"], "mp4");
            
            ProgramIsBusy = false;
        }
        private static void OnChange(object sender, FileSystemEventArgs e)
        {
            //
           ProcessQueue.Enqueue(e.FullPath);
            while (ProgramIsBusy == false)
            {
                var inputPath = ProcessQueue.Peek();
                ConvertVideoFromQueue(inputPath);
                ProgramIsBusy = true;
            }
        }

        protected override void OnStart(string[] args)
        {
           InitializeFileSystemWatcher();
        }

       

        protected override void OnStop()
        {

        }

    }
}
