﻿using Microsoft.Extensions.Configuration;
using System.IO;

namespace gRPCNet.ServerAPI
{
    public interface IFileLogger
    {
        void WriteProgramLog(string message);

        void WriteErrorLog(string message);
    }

    public class FileLogger : IFileLogger
    {
        private static object locker = new object();

        private readonly IConfiguration _configuration;
        private readonly string _pathToContentRoot;

        public FileLogger(IConfiguration configuration, string pathToContentRoot)
        {
            _configuration = configuration;
            _pathToContentRoot = pathToContentRoot;
        }

        public void WriteErrorLog(string message)
        {
            var path = $"{_pathToContentRoot}{Path.DirectorySeparatorChar}{_configuration.GetSection("AppSettings").GetValue<string>("ErrorLog")}";
            lock (locker)
            {
                using (var sw = File.AppendText(path))
                {
                    sw.WriteLine(message);
                    sw.WriteLine();
                }
            }
        }

        public void WriteProgramLog(string message)
        {
            var path = $"{_pathToContentRoot}{Path.DirectorySeparatorChar}{_configuration.GetSection("AppSettings").GetValue<string>("ProgramLog")}";
            lock (locker)
            {
                using (var sw = File.AppendText(path))
                {
                    sw.WriteLine(message);
                    sw.WriteLine();
                }
            }
        }
    }
}