using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BlackDuckCMDTools
{
    public class FastLogger
    {
        StreamWriter _sw;        

        public FastLogger(string filepath)
        {            
           _sw = new StreamWriter(filepath, true);
        }
        
        public void LogToFile (string text)
        {
            _sw.WriteLine(text);
        }

        public void StopLogging()
        {
            _sw.Close();
            _sw.Dispose();
        }
    }
}
