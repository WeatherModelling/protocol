using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace GUI.GNUPlot
{
    internal abstract class Series
    {
        #region ObjectFactory-like pattern

        private static Dictionary<string, Type> SeriesTypes = new Dictionary<string, Type>();
        private static readonly Type[] constructorArguments = new Type[] { typeof(JObject), typeof(JArray) };
        public static void RegisterSeriesType(string name, Type type)
        {
            if (!type.IsSubclassOf(typeof(Series)))
            {
                throw new ArgumentException($"Class {type.FullName} must be derived from {typeof(Series).FullName}");
            }
            if (type.GetConstructor(constructorArguments) == null)
            {
                throw new ArgumentException($"Class {type.FullName} does not contain a constructor with appropriate arguments");
            }
            SeriesTypes.Add(name, type);
        }
        public static Series Construct(JObject plotConfig, JArray outputConfig)
        {
            string typename = plotConfig["type"].ToString();
            if (!SeriesTypes.ContainsKey(typename))
            {
                throw new InvalidOperationException($"Unsupported series type name '{typename}'");
            }
            var ctor = SeriesTypes[typename].GetConstructor(constructorArguments);
            object series = ctor.Invoke(new object[] { plotConfig, outputConfig });
            return (Series)series;
        }
        #endregion
        
        protected static string GNUPlotTemplatesDirectory = "plotting";
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
                    throw new ArgumentException($"Variable '{name}' not found in solver output");
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