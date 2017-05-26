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
    class ApplicationSettings
    {
        // application working directory
        public static string WorkingDirectory => workingDirectory;

        // GNUPlot executable location
        public static string GNUPlotExecutable => gNUPlotExecutable;

        private static string workingDirectory = GetWorkingDirectory();
 
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


        private static string gNUPlotExecutable =
             ConfigurationManager.AppSettings["GNUPlotExecutable"] ??
            "C:/Program Files/gnuplot/bin/gnuplot.exe";

        public static void SaveSettings()
        {
            ConfigurationManager.AppSettings["GNUPlotExecutable"] = GNUPlotExecutable;
            ConfigurationManager.AppSettings["WorkingDirectory"] = WorkingDirectory;
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configFile.Save(ConfigurationSaveMode.Modified);
        }

        public static void ReloadSettings()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }
    }
}
