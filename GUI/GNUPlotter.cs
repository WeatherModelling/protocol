using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GUI.GNUPlot;
namespace GUI
{
    class GNUPlotter : INotifyPropertyChanged
    {
        private static int last_id = 0;
        private readonly int id = last_id++;

        public readonly string GNUPlotExecutable = "C:/Program Files/gnuplot/bin/gnuplot.exe";
        private Series series;

        public GNUPlotter(Series series)
        {
            this.series = series;
        }

        public ImageSource Image
        {
            get; private set;
        }

        // load bitmap and cache it in memory
        private void SetImage(string uri)
        {
            if (string.IsNullOrEmpty(uri.ToString()))
            {
                throw new ArgumentException("unable to parse url");
            }
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(uri);
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            bi.Freeze();
            Image = bi;
            NotifyPropertyChanged("Image");
        }

        internal void Update(string res)
        {
            using (Process process = new Process())
            {
                string filename = MainWindow.WorkingDir + "/temp/" +  Guid.NewGuid().ToString() + ".png";

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = GNUPlotExecutable;
                process.Start();
                string script = 
                    series.GetScript()
                    .Replace("{{path}}", filename);
                process.StandardInput.WriteLine(script);
                process.StandardInput.WriteLine("#" + res);
                process.StandardInput.WriteLine("e");
                process.StandardInput.Close();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                SetImage(filename);
                File.Delete(filename);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
