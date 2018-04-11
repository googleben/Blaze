using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace XNA3D
{
#if WINDOWS || LINUX
    
    public static class Program
    {

        public static Logger log = new Logger();

        //entry point
        [STAThread]
        static void Main()
        {
            log.Log("Game startup");
            try {
                using (var game = new Blaze())
                    game.Run();
            } catch (Exception e) {
                log.Log(e.ToString());

                var now = DateTime.Now;
                var logName = String.Format("ERROR {0}-{1}-{2} {3}-{4}-{5}.log", now.Month, now.Day, now.Year, now.Hour, now.Minute, now.Second);
                var logLocation = @"./logs/" + logName;
                using (var file = new StreamWriter(File.Create(logLocation)))
                {
                    file.WriteLine(e.ToString());
                }

                var ans = MessageBox.Show(e.ToString()+"\n\nEmail crash report?", "An error has occurred. Email crash report?", MessageBoxButtons.YesNo);
                if (ans==DialogResult.Yes) System.Diagnostics.Process.Start("mailto:crash@CognitiveThoughtMedia.com?subject=Blaze+Crash+Report?body="+e.ToString().Replace(" ", "%20").Replace("\n", "%0A").Replace("\t", "%20"));
            }
        }
    }
#endif
}
