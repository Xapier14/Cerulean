using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cerulean.Analyzer
{
    internal class Logger
    {
        private const string LOG_DIRECTORY = @"D:\Cerulean\Logs";
        private static long _logCount = 1;

        public static void WriteLine(string message, params object[] args)
        {
            using var fileStream = File.OpenWrite($"{LOG_DIRECTORY}\\{_logCount}---{Guid.NewGuid()}.txt");
            _logCount++;
            using var writer = new StreamWriter(fileStream);
            writer.WriteLine(message, args);
            writer.Close();
        }
    }
}
