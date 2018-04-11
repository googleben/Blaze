using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNA3D
{
    public class Logger
    {

        private string logName;
        private string logLocation;

        public Logger()
        {
            if (!Directory.Exists(@"./logs/")) Directory.CreateDirectory(@"./logs");
            var now = DateTime.Now;
            logName = String.Format("{0}-{1}-{2} {3}-{4}-{5}.log", now.Month, now.Day, now.Year, now.Hour, now.Minute, now.Second);
            logLocation = @"./logs/" + logName;
            using (File.Create(logLocation)) { }
        }

        //log a message to the log file
        public void Log(string message)
        {
            using (StreamWriter sw = new StreamWriter(new FileStream(logLocation, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))) {
                sw.WriteLine(String.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), message));
            }
        }

    }
}
