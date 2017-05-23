using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    sealed class Problem
    {
        private Problem(JObject problem)
        {
            // set up config
            this.json = problem;
            // create solver connection
            SolverConnector = SolverConnector.Construct(json);

        }
        // json problem description
        public readonly JObject json;

        // connector for current problem solver
        public SolverConnector SolverConnector { get; }

        // human-readable problem name
        public override string ToString()
        {
            return json["name"].ToString();
        }

        static public List<Problem> ReadRirectory(string problemsDirectory)
        {
            return new List<Problem>(
                from filename
                in Directory.GetFiles(problemsDirectory, "*.json")
                select new Problem(
                    JObject.Parse(
                    File.ReadAllText(filename
                )))
            );


        }
    }
}
