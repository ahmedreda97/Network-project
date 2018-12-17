using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Logger
    {
        public static void LogException(Exception ex)
        {
            string path = "log.txt";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(ex.ToString()+" " +DateTime.Now.ToString());
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(ex.ToString() + " " + DateTime.Now.ToString());
                }
            }
            // TODO: Create log file named log.txt to log exception details in it
            // for each exception write its details associated with datetime 
        }
    }
}
