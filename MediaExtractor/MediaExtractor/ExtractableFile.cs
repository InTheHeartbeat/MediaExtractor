using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaExtractor
{
    public abstract class ExtractableFile
    {
        public FileInfo File { get; set; }

        public ExtractProgressModel CurrentProgressState { get; set; }

        public event Action<ExtractProgressModel> ProgressChanged;
        public event Action<ExtractProgressModel> Completed;

        protected readonly BackgroundWorker BackgroundWorker;

        public ExtractableFile(FileInfo file)
        {            
            File = file;
            BackgroundWorker = new BackgroundWorker();
            BackgroundWorker.WorkerReportsProgress = true;
            BackgroundWorker.DoWork += _backgroundWorker_DoWork;
            BackgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            BackgroundWorker.RunWorkerCompleted += (sender, args) => { Completed?.Invoke(CurrentProgressState); };
        }

        public void LaunchExtract()
        {
            BackgroundWorker.RunWorkerAsync();
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CurrentProgressState = e.UserState as ExtractProgressModel;
            ProgressChanged?.Invoke(CurrentProgressState);
        }

        protected abstract void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e);
    }
}
