using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI.Extensions
{
    internal static class FileInfoExtensions
    {
        public static int TryDelete(this FileInfo fileInfo, string? appendMessage = null)
        {
            var status = 0;
            try
            {
                fileInfo.Delete();
            }
            catch (Exception ex)
            {
                if (appendMessage is { })
                    appendMessage += " ";
                ColoredConsole.WriteLine($"{appendMessage}Could not delete file '{fileInfo.FullName}'. {ex.Message}");
                status++;
            }

            return status;
        }
    }
}
