using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    class ApplicationSettings : INotifyPropertyChanged
    {
        public static ApplicationSettings Instance { get; } = new ApplicationSettings();

        private ApplicationSettings() { }


        #region GNUPlotExecutable
        private string gNUPlotExecutable =
          ConfigurationManager.AppSettings["GNUPlotExecutable"] ??
         "C:/Program Files/gnuplot/bin/gnuplot.exe";

        // GNUPlot executable location
        public string GNUPlotExecutable
        {
            get => gNUPlotExecutable;
            set
            {
                gNUPlotExecutable = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region WorkingDirectory
        // application working directory
        private string workingDirectory = GetWorkingDirectory();

        private static string GetWorkingDirectory()
        {
            var path = ConfigurationManager.AppSettings["WorkingDirectory"] ??
                $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\WeatherGUI\\workingDirectory";
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Working directory '{path}' not found");
            }
            return path;
        }
        public string WorkingDirectory
        {
            get => workingDirectory;
            set
            {
                workingDirectory = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region MaxFPS
        // maximum framerate for dynamic scenes
        private int maxFPS = 
            Convert.ToInt32(ConfigurationManager.AppSettings["MaxFPS"] ?? "5") ;
        public int MaxFPS
        {
            get => maxFPS;
            set
            {
                maxFPS = value;
                NotifyPropertyChanged();
            }
        }

        #endregion



        public void SaveSettings()
        {
            ConfigurationManager.AppSettings["GNUPlotExecutable"] = GNUPlotExecutable;
            ConfigurationManager.AppSettings["WorkingDirectory"] = WorkingDirectory;
            ConfigurationManager.AppSettings["MaxFPS"] = Convert.ToString(MaxFPS);
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configFile.Save(ConfigurationSaveMode.Modified);
        }

        public void ReloadSettings()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
