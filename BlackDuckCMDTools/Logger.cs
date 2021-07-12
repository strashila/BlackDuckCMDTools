using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BlackDuckCMDTools
{
    public static class Logger
    {
        public static void Log(string filePath, string text)
        {
            var path = @filePath;

            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    
                }
            }


            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(text);
            }

        }
    }
}
