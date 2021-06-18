using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetZero.Automated.UI.Tests.Utils
{
    public class Logger
    {
        private string FileName;
        private string LogDirectory;
        public string FilePath;
        public Logger()
        {
            this.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "testresults");
            this.FileName = $"{DateTime.Now.ToString("ddMM-HHmmss")}-Log.txt";
            this.FilePath = Path.Combine(LogDirectory, FileName);
        }
        public void LogToFile(string logText)
        {
            using(StreamWriter w = File.AppendText(FilePath))
            {
                w.WriteLine(logText);
            }
        }

        public void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
            var logText = $"{timestamp} | LOG INFO : {message}";
            Console.WriteLine(logText);
            LogToFile(logText);
        }

        public void LogMessage(string message)
        {
            var logText = $"                                  {message}";
            Console.WriteLine(logText);
            LogToFile(logText);
        }
    }
}
