using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public static bool ProgramIsBusy;

        private static void ConvertVideoFromQueue(string inputPath)
        {
            //Pick up file paths of files that are created in WatchingPath directory then pass them into converter.
            var fileConverter = new NReco.VideoConverter.FFMpegConverter();
            //Parse inputPath and Create a directory in the OutputPath for the file.
            var fileName = inputPath.Split('\\').Last();

            var newDirectoryPath = string.Format(@"{0}\{1}", ConfigurationSettings.AppSettings["OutputPath"], fileName);
            var outputDirectory = Directory.CreateDirectory(newDirectoryPath);

            fileConverter.ConvertMedia(inputPath, outputDirectory.ToString(), "mp4");
           
            ProgramIsBusy = false;
            
        }
        private static void OnChange(object sender, FileSystemEventArgs e)
        {
           ProcessQueue.Enqueue(e.FullPath);
            while (ProgramIsBusy == false)
            {
                //Check for values in the ProcessQueue
                if (ProcessQueue.Peek() != null)
                {
                    var inputPath = ProcessQueue.Peek();
                    ConvertVideoFromQueue(inputPath);
                    ProcessQueue.Dequeue();
                    ProgramIsBusy = true;
                }
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
