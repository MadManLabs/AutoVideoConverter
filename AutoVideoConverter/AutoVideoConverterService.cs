using System;
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

        private static bool FileIsNotBusy(string filePath)
        {
            FileStream stream = null;
            var fileInfo = new FileInfo(filePath);
            try
            {
                stream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public static bool ProgramIsBusy;

        private static void ConvertVideoFromQueue(string inputPath)
        {
                ProgramIsBusy = true;
                ProcessQueue.Dequeue();
            //Pick up file paths of files that are created in WatchingPath directory then pass them into converter.
            var fileConverter = new NReco.VideoConverter.FFMpegConverter();
            //Parse inputPath and Create a directory in the OutputPath for the file.
            var filePath = inputPath.Split('\\').Last();
            var outputPath = string.Format(@"{0}\{1}", ConfigurationSettings.AppSettings["OutputPath"], filePath);
                fileConverter.ConvertMedia(inputPath, outputPath, "mp4");
                ProgramIsBusy = false;
              
        }

        private static void PersistQueueOnStop()
        {
            
        }
        private static void OnChange(object sender, FileSystemEventArgs e)
        {
           ProcessQueue.Enqueue(e.FullPath);
            while (ProgramIsBusy == false)
            {
                //Check for values in the ProcessQueue
                if (ProcessQueue.Peek() != null)
                {
                    if (FileIsNotBusy(ProcessQueue.Peek()))
                    {
                        ConvertVideoFromQueue(ProcessQueue.Peek());
                    }
                }
            }
        }

        protected override void OnStart(string[] args)
        {
           InitializeFileSystemWatcher();
        }

       

        protected override void OnStop()
        {
            PersistQueueOnStop();
            Environment.Exit(0);
        }

    }
}
