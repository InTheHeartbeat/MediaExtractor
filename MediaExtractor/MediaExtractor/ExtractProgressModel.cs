using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaExtractor
{
    public class ExtractProgressModel
    {
        public FileInfo File { get; set; }
        public string GeneratedPath { get; set; }

        public int TotalMessages { get; set; }
        public int TotalAttachments { get; set; }
        public int TotalEntries => TotalMessages + TotalAttachments;

        public int SuccessAttachments { get; set; }
        public int SuccessMessages { get; set; }

        public int Failed { get; set; }

        public int TotalHandled => SuccessMessages + SuccessAttachments + Failed;

        public TimeSpan Estimate { get; set; }
        public TimeSpan Elapsed { get; set; }

        public int TotalPng { get; set;}
        public int TotalJpg { get; set; }
        public int TotalDoc { get; set; }
    }
}
