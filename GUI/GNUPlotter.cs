﻿using Newtonsoft.Json.Linq;
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
using System.Configuration;

namespace GUI
{
    class GNUPlotter : INotifyPropertyChanged
    {
        private static int last_id = 0;
        private readonly int id = last_id++;


        private static Process GNUPlotProcess = null;

        private Series series;

        private void StartProcess()
        {
            if (GNUPlotProcess != null)
            {
                return;
            }
            GNUPlotProcess = new Process();
            GNUPlotProcess.StartInfo.UseShellExecute = false;
            GNUPlotProcess.StartInfo.RedirectStandardOutput = true;
            GNUPlotProcess.StartInfo.RedirectStandardInput = true;
            GNUPlotProcess.StartInfo.RedirectStandardError = true;
            GNUPlotProcess.StartInfo.CreateNoWindow = true;
            GNUPlotProcess.StartInfo.FileName = ApplicationSettings.Instance.GNUPlotExecutable;
            GNUPlotProcess.Start();
        }


        public GNUPlotter(Series series)
        {
            this.series = series;
            StartProcess();
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
            // used to catch the moment when GNUPlot 'plot' call finishes
            string magicWord = "xyzzy";
            string filename =  $"{Path.GetTempPath()}\\{Guid.NewGuid().ToString()}.png";

            string script =
                series.GetScript()
                .Replace("{{path}}", filename);
            GNUPlotProcess.StandardInput.WriteLine(script);
            GNUPlotProcess.StandardInput.WriteLine("#" + res);
            GNUPlotProcess.StandardInput.WriteLine("e");
            // when gnuplot is configured to write an output file it releases previous output file
            GNUPlotProcess.StandardInput.WriteLine("set output 'x.txt'");
            // print a marker to stderr
            GNUPlotProcess.StandardInput.WriteLine($"print '{magicWord}'");
            GNUPlotProcess.StandardInput.Flush();
            //GNUPlotProcess.StandardInput.Close();
            //Thread.Sleep(1);
            // wait for marker
            while (GNUPlotProcess.StandardError.ReadLine() != magicWord) ;
            //GNUPlotProcess.WaitForExit();
            SetImage(filename);
            File.Delete(filename);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
