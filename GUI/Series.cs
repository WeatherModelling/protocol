using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace GUI.GNUPlot
{
    internal abstract class Series
    {
        protected static string GNUPlotTemplatesDirectory = "/plotting/";
        public int Dimensions { get; protected set; }

        protected List<int> FindOrderOfVariablesInOutput(JArray outputConfig, IEnumerable<string> variableNames)
        {
            List<int> res = new List<int>();
            foreach (string name in variableNames)
            {
                int index = 0;
                for (int i = 0; i < outputConfig.Count; i++)
                {
                    if ((outputConfig[i] as JObject)["name"].Value<string>() == name)
                    {
                        // GNUPlot indexes are counted from 1
                        index = i + 1;
                        break;
                    }
                }
                // if not found
                if (index == 0)
                {
                    throw new ArgumentException("Variable '" + name + "' not found in solver output");
                }
                res.Add(index);
            }
            return res;
        }

        protected bool scriptTemplateReady = false;
        protected string scriptTemplate;

        public virtual string GetScript()
        {
            if (!scriptTemplateReady)
            {
                throw new InvalidOperationException("script template not ready");
            }
            return scriptTemplate;
        }
    }
}