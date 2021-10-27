using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WinGridApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static readonly object lockObj = new object();
        public static void Log(string log)
        {
            lock (lockObj)
            {
                using (var file = File.AppendText("WinGridLog.txt"))
                {
                    file.WriteLine($"[{DateTime.Now}] > {log}");
                }
            }
        }
    }
}
