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
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace MediaExtractor
{
    static class StopWatchUtils
    {
        /// <summary>
        /// Gets estimated time on compleation. 
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="counter"></param>
        /// <param name="counterGoal"></param>
        /// <returns></returns>
        public static TimeSpan GetEta(this Stopwatch sw, int counter, int counterGoal)
        {
            /* this is based off of:
             * (TimeTaken / linesProcessed) * linesLeft=timeLeft
             * so we have
             * (10/100) * 200 = 20 Seconds now 10 seconds go past
             * (20/100) * 200 = 40 Seconds left now 10 more seconds and we process 100 more lines
             * (30/200) * 100 = 15 Seconds and now we all see why the copy file dialog jumps from 3 hours to 30 minutes :-)
             * 
             * pulled from http://stackoverflow.com/questions/473355/calculate-time-remaining/473369#473369
             */
            if (counter == 0) return TimeSpan.Zero;
            float elapsedMin = ((float)sw.ElapsedMilliseconds / 1000) / 60;
            float minLeft = (elapsedMin / counter) * (counterGoal - counter); //see comment a
            TimeSpan ret = TimeSpan.FromMinutes(minLeft);
            return ret;
        }
    }

    class Program
    {
        private static List<HtmlExtractableFile> _files;
        static void Main(string[] args)
        {

            //var doc = new HtmlDocument();
            //doc.Load();
            

            DirectoryInfo di = new DirectoryInfo(@"F:\HDDOld\3");
            _files = di.GetFiles("*.html").Select(path => new HtmlExtractableFile(path, @"G:\vkph\1")).ToList();
            _files.ForEach(exFile =>
            {
                exFile.ProgressChanged += Assd_ProgressChanged;
                exFile.Completed += Assd_Completed;
                exFile.LaunchExtract();
            });

            //HtmlExtractableFile assd =new HtmlExtractableFile(new FileInfo(@"F:\HDDOld\3\messages_Eugene Fonblaser(162607262).html"), @"G:\vkph\1");


            //assd.ProgressChanged += Assd_ProgressChanged;
            //assd.Completed += Assd_Completed;

            //assd.LaunchExtract();

            
            //MatchCollection collection = Regex.Matches(File.ReadAllText(),
            //    @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");

            //match = new Match[collection.Count];

            //collection.CopyTo(match, 0);

            //BackgroundWorker worker = new BackgroundWorker();
            //worker.DoWork += Worker_DoWork;
            //worker.RunWorkerAsync();
            

            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
        }

        private static void Assd_Completed(ExtractProgressModel obj)
        {
            if(obj == null){return;}

            if (!Directory.Exists(obj.GeneratedPath))
            {
                Directory.CreateDirectory(obj.GeneratedPath);
            }
            FileInfo log = new FileInfo(Path.Combine(obj.GeneratedPath, "log.txt"));
            using (StreamWriter sw = log.CreateText())
            {
                sw.WriteLine($"Current file: {obj.File.Name}");
                sw.WriteLine($"Progress:     {Math.Round(((double)obj.TotalHandled / (double)obj.TotalEntries) * 100.0, 1)}%");
                sw.WriteLine();
                sw.WriteLine("---------------------------------------------------------------------------");
                sw.WriteLine($"Estimate:                                {new DateTime(obj.Estimate.Ticks):T}");
                sw.WriteLine($"Elapsed:                                 {new DateTime(obj.Elapsed.Ticks):T}");

                sw.WriteLine($"TotalEntries/TotalHandled:               {obj.TotalEntries}/{obj.TotalHandled}");
                sw.WriteLine($"TotalMessages/SuccessMessages:           {obj.TotalMessages}/{obj.SuccessMessages}");
                sw.WriteLine($"TotalAttachments/SuccessAttachments:     {obj.TotalAttachments}/{obj.SuccessAttachments}");
                sw.WriteLine($"Success/Failed:                          {obj.SuccessMessages + obj.SuccessAttachments}/{obj.Failed}");
                sw.WriteLine($"Failed ratio:                            {Math.Round(((double)obj.Failed / ((double)obj.SuccessMessages + obj.SuccessAttachments)) * 100.0, 1)}%");
                sw.WriteLine();
                sw.WriteLine($"Jpg: {obj.TotalJpg}");
                sw.WriteLine($"Png: {obj.TotalPng}");
                sw.WriteLine($"Doc: {obj.TotalDoc}");
            }
        }

        static object locker = new object();
        private static void Assd_ProgressChanged(ExtractProgressModel obj)
        {
            lock (locker)
            {
                int index = _files.FindIndex(f => f.File.Name == obj.File.Name);

                Console.SetCursorPosition(0, index*17);
                Console.WriteLine();
                Console.WriteLine($"Current file: {obj.File.Name}");
                Console.WriteLine(
                    $"Progress:     {Math.Round(((double) obj.TotalHandled / (double) obj.TotalEntries) * 100.0, 1)}%");
                Console.WriteLine();
                Console.WriteLine("---------------------------------------------------------------------------");
                Console.WriteLine($"Estimate:                                {new DateTime(obj.Estimate.Ticks):T}");
                Console.WriteLine($"Elapsed:                                 {new DateTime(obj.Elapsed.Ticks):T}");

                Console.WriteLine($"TotalEntries/TotalHandled:               {obj.TotalEntries}/{obj.TotalHandled}");
                Console.WriteLine(
                    $"TotalMessages/SuccessMessages:           {obj.TotalMessages}/{obj.SuccessMessages}");
                Console.WriteLine(
                    $"TotalAttachments/SuccessAttachments:     {obj.TotalAttachments}/{obj.SuccessAttachments}");
                Console.WriteLine(
                    $"Success/Failed:                          {obj.SuccessMessages + obj.SuccessAttachments}/{obj.Failed}");
                Console.WriteLine(
                    $"Failed ratio:                            {Math.Round(((double) obj.Failed / ((double) obj.SuccessMessages + obj.SuccessAttachments)) * 100.0, 1)}%");
                Console.WriteLine();
                Console.WriteLine($"Jpg: {obj.TotalJpg}");
                Console.WriteLine($"Png: {obj.TotalPng}");
                Console.WriteLine($"Doc: {obj.TotalDoc}");
                Console.WriteLine();
            }
        }       
    }
}
