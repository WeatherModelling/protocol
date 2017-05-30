using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.GNUPlot
{
    internal class HeatmapSeries : Series
    {

        public HeatmapSeries(JObject plotConfig, JArray outputConfig)
        {
            Dimensions = 3;
            scriptTemplate = System.IO.File.ReadAllText(
                $"{ApplicationSettings.Instance.WorkingDirectory}\\{GNUPlotTemplatesDirectory}\\heatmap.gpl"
            );

            // find plotting varibles
            var variableOrder = FindOrderOfVariablesInOutput(
                outputConfig, new[] {
                    plotConfig["xvalues"].Value<string>(),
                    plotConfig["yvalues"].Value<string>(),
                    plotConfig["zvalues"].Value<string>()
                });
            scriptTemplate = scriptTemplate
                .Replace("{{x}}", variableOrder[0].ToString())
                .Replace("{{y}}", variableOrder[1].ToString())
                .Replace("{{z}}", variableOrder[2].ToString())
                .Replace("{{caption}}", plotConfig["caption"]?.ToString() ?? "")
             ;
            scriptTemplateReady = true;
        }
    }
}
