using System;
using System.Text;
using System.Threading.Tasks;

namespace gRPCNet.ServerAPI.CommonServices.Utils
{
    public class Logger : ILogger
    {
        private static readonly object _syncRoot = new object();

        private const string LogPath = "./logs";
        private const string ExceptionLogPath = "./exceptions";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">path to folder!</param>
        /// <param name="expath">path to folder!</param>
        public Logger(string path = LogPath, string expath = ExceptionLogPath)
        {
            this.Path = path;
            this.ExceptionPath = expath;
        }

        public string Path { get; set; }
        public string ExceptionPath { get; set; }

        public void InfoLog(string log, string ip = "")
        {
            this.SaveLog("INFO", log, ip);
        }

        public void WarningLog(string log, string ip = "")
        {
            this.SaveLog("WARNING", log, ip);
        }

        public void ErrorLog(string log, string ip = "")
        {
            this.SaveLog("ERROR", log, ip);
        }

        public void LogException(string exceptionMessage)
        {
            DateTime now = DateTime.Now;
            var task = Task.Run(() =>
            {
                string logPath = $"{this.ExceptionPath}/log-{now.Year}-{now.Month}-{now.Day}.txt";
                string data = $"[{now:yyyy-MM-dd HH:mm:ss.fff}] {exceptionMessage}{Environment.NewLine}{Environment.NewLine}";
                if (!System.IO.Directory.Exists(this.ExceptionPath))
                    lock (_syncRoot)
                    {
                        if (!System.IO.Directory.Exists(this.ExceptionPath))
                            System.IO.Directory.CreateDirectory(this.ExceptionPath);
                    }
                lock (_syncRoot)
                {
                    System.IO.File.AppendAllText(logPath, data, Encoding.UTF8);
                }
            });
        }

        private void SaveLog(string type, string log, string ip)
        {
            DateTime now = DateTime.Now;
            var task = Task.Run(() =>
            {
                string logPath = $"{this.Path}/log-{now.Year}-{now.Month}-{now.Day}.txt";
                string data = $"[{type}] [{now}] [{ip}] " + log + Environment.NewLine;
                if (!System.IO.Directory.Exists(this.Path))
                    lock (_syncRoot)
                    {
                        if (!System.IO.Directory.Exists(this.Path))
                            System.IO.Directory.CreateDirectory(this.Path);
                    }
                lock (_syncRoot)
                {
                    System.IO.File.AppendAllText(logPath, data, Encoding.UTF8);
                }
            });
        }

        protected string GetDateFormat()
        {

            return $"{DateTime.Now.Year}-{this.FormatNumber(DateTime.Now.Month)}-{this.FormatNumber(DateTime.Now.Day)}  {this.FormatNumber(DateTime.Now.Hour)}:{this.FormatNumber(DateTime.Now.Minute)}:{this.FormatNumber(DateTime.Now.Second)}:{this.FormatNumber(DateTime.Now.Millisecond)}";
        }

        private string FormatNumber(int number)
        {
            if (number < 10)
            {
                return "0" + number;
            }
            return number.ToString();
        }
    }
}
