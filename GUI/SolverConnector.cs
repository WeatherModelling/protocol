using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    abstract class SolverConnector
    {
        public bool Connected { get; protected set; } = false;
        public bool CalculationStarted { get; private set; } = false;
        public bool Restartable { get; private set; }
        public bool Dynamic { get; private set; }
        public readonly JObject settings;
        public JArray OutputConfiguration { get; private set; }

        #region ObjectFactory-like pattern
        static Dictionary<string, Type> SolverConnectorTypes = new Dictionary<string, Type>();
        public static void RegisterSolverConnectorType(string name, Type type)
        {
            SolverConnectorTypes[name] = type;
        }
        public static SolverConnector Construct(JObject jo)
        {
            string typename = jo["solver"]["type"].ToString();
            if (!SolverConnectorTypes.ContainsKey(typename))
            {
                throw new InvalidOperationException("Unsupported solver type");
            }
            var ctor = SolverConnectorTypes[typename].GetConstructor(new Type[] { typeof(JObject) });
            object solver =  ctor.Invoke(new object[] { jo });
            return (SolverConnector)solver;
        }
        #endregion

        protected abstract string  ExecuteProcedure(string json);
        public abstract void Connect();
        public abstract void Disconnect();

        public SolverConnector(JObject settings)
        {
            this.settings = settings;
        }

        public void Handshake()
        {
            try
            {
                string json;

                // check protocol version
                json = ExecuteProcedure("{\"function\":\"version\"}");
                if (JObject.Parse(json)["version"].ToObject<int>() != 1)
                {
                    throw new InvalidOperationException("protocol version unsupported");
                }
                // get solver description

                json = ExecuteProcedure("{\"function\":\"getCapabilities\"}");
                JObject capabilities = JObject.Parse(json);


                var features = capabilities["features"].ToObject<List<string>>();
                Restartable = features.Contains("restartable");
                Dynamic = features.Contains("dynamic");
                JArray resultsMetadata = (JArray)capabilities["results"];

                // check whether we have correspodence in initial values in settings and solver capabilities

                foreach (JObject obj in capabilities["initials"])
                {
                    // find appropriate variable
                    var settingsToken = settings["initials"].First(
                        (JToken t) => JToken.DeepEquals(((JObject)t)["name"], obj["name"])
                    );

                    // compare types
                    if (!JToken.DeepEquals(
                        obj["type"],
                        settingsToken["type"]
                        ))
                    {
                        throw new InvalidOperationException("initial data type values not correspond for " + obj.ToString());
                    }
                }
                OutputConfiguration = capabilities["results"] as JArray;
                Connected = true;
            }
            catch (InvalidOperationException ex)
            {
                Connected = false;
                throw ex;
            }
        }

        public void InitializeSolver()
        {
            if (CalculationStarted && !Restartable)
            {
                throw new InvalidOperationException("A non-restartable calculation already started. Disconnect and connect again.");
            }
            JObject parameters = new JObject
            {
                ["function"] = "initSolver",
                ["params"] = new JObject()
            };
            // form parameters from initials
            foreach (JObject obj in settings["initials"])
            {
                parameters["params"][obj["name"].ToString()] = obj["value"];
            }

            string response = ExecuteProcedure(parameters.ToString());
            JObject initResults = JObject.Parse(response);

            CalculationStarted = true;
        }

        public bool Evolve(double finalTime)
        {
            JObject parameters = new JObject
            {
                ["function"] = "evolve",
                ["finalTime "] = finalTime
            };
            return Evolve(parameters);
        }

        public bool Evolve(int timeSteps)
        {
            JObject parameters = new JObject
            {
                ["function"] = "evolve",
                ["timeStepsNumber"] = timeSteps
            };
            return Evolve(parameters);

        }

        private bool Evolve(JObject parameters)
        {
            if (!Dynamic)
            {
                throw new InvalidOperationException("Evolve is available only for dynamic models");
            }
            if (!CalculationStarted)
            {
                throw new InvalidOperationException("Calculation is not started");
            }
            
            string res = ExecuteProcedure(parameters.ToString());
            JObject result = JObject.Parse(res);
            return result["success"].Value<bool>();

        }

        public string GetResults()
        {
            if (!CalculationStarted)
            {
                throw new InvalidOperationException("Calculation is not started");
            }
            JObject parameters = new JObject
            {
                ["function"] = "getResults",
                ["names"] = "all"
            };
            return ExecuteProcedure(parameters.ToString());
        }
    }
}
