using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace MediaExtractor
{
    public class HtmlExtractableFile : ExtractableFile
    {
        private const string DivSelector = "div";
        private const string ClassAttributeName = "class";
        private const string MsgItemClassName = "msg_item";
        private const string AttachmentClassName = "attacment";
        private const string FromClassName = "from";

        private const string DateRegex = @"(\d{4}).(\d{2}).(\d{2}) (\d{2}):(\d{2}):(\d{2})";
        private const string UrlRegex = @"(http(s?):)([/|.|\w|\s|-])*\.(?:jpg|png)";

        private readonly string _targetFolder;        

        public HtmlExtractableFile(FileInfo file, string targetFolder) : base(file)
        {
            _targetFolder = targetFolder;
        }

        protected override void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ExtractProgressModel progress = new ExtractProgressModel();
            DateTime started = DateTime.Now;
            Stopwatch etaWatch = new Stopwatch();

            var extractEnties = ParseExtractableFile(File.FullName);

            progress.TotalAttachments = extractEnties.Sum(entry => entry.Value.Count);
            progress.TotalMessages = extractEnties.Count;
            progress.File = File;
            
            progress.GeneratedPath = Path.Combine(_targetFolder, File.Name); 

            using (WebClient client = new WebClient())
            {
                client.Proxy = new WebProxy("54.39.144.247:3128");
                extractEnties.ForEach(entry =>
                {
                    etaWatch.Start();

                    entry.Value.ForEach(url =>
                    {
                        try
                        {
                            string type = url.AbsoluteUri.Contains("jpg") ? "jpg" :
                                          url.AbsoluteUri.Contains("png") ? "png" : "doc";

                            switch (type)
                            {
                                case "jpg":
                                    progress.TotalJpg++;
                                    break;
                                case "png":
                                    progress.TotalPng++;
                                    break;
                                case "doc":
                                    progress.TotalDoc++;
                                    break;
                            }

                            string path = Path.Combine(_targetFolder, File.Name, type);                           
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            
                            var filePath = Path.Combine(path,
                                entry.Key.ToString("dd-MM-yyyy-hh-mm-ss") + url.AbsoluteUri.Split('/').Last());

                            client.DownloadFile(url, filePath);                            
                            progress.SuccessAttachments++;
                        }
                        catch(Exception ee)
                        {
                            progress.Failed++;
                        }

                        progress.Elapsed = DateTime.Now - started;
                        progress.Estimate = etaWatch.GetEta(progress.TotalHandled, progress.TotalEntries);
                        CurrentProgressState = progress;
                        BackgroundWorker.ReportProgress(Math.Sign(progress.TotalHandled / (double)progress.TotalEntries * 100.0), progress);
                    });
                    progress.SuccessMessages++;
                    //BackgroundWorker.ReportProgress(Math.Sign(progress.TotalHandled / (double)progress.TotalEntries * 100.0), progress);
                });
            }

            CurrentProgressState = progress;
        }

        private List<KeyValuePair<DateTime, List<Uri>>> ParseExtractableFile(string fileName)
        {
            var doc = new HtmlDocument();
            doc.Load(fileName);


            var msgWithAttachment = doc.DocumentNode.Descendants(DivSelector)
                .Where(div => div.Attributes[ClassAttributeName]?.Value == MsgItemClassName)
                .Where(div => div.ChildNodes.Any(childDiv => childDiv.Attributes[ClassAttributeName]?.Value == AttachmentClassName))
                .ToList();

            var datePairAttachments = msgWithAttachment.Select(msg =>
                    new KeyValuePair<string, List<HtmlNode>>
                    (Regex.Match(msg.ChildNodes
                                     .FirstOrDefault(child => child.Attributes != null
                                                              && child.Attributes[ClassAttributeName]?.Value == FromClassName)?.InnerHtml ?? "", DateRegex)
                            .Value,
                        msg.ChildNodes.Where(child => child.Attributes[ClassAttributeName]?.Value == AttachmentClassName).ToList()))
                .ToList();

            var datePairAttachmentUris = datePairAttachments.Select(dpa => new KeyValuePair<string, List<string>>
                (dpa.Key,
                    dpa.Value.Select(val => Regex.Match(val.InnerHtml, UrlRegex).Value).ToList()))
                .ToList();

            return datePairAttachmentUris.Select(dpa =>
            {
                DateTime.TryParse(dpa.Key, out DateTime taken);
                List<Uri> uris = dpa.Value.Where(url => !String.IsNullOrWhiteSpace(url)).Select(url => new Uri(url))
                    .ToList();

                return new KeyValuePair<DateTime, List<Uri>>
                (taken, uris);
            }).ToList();
        }
    }
}
