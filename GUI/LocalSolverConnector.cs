using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GUI
{
    class LocalSolverConnector : SolverConnector,IDisposable
    {
        Process process;

        protected override string ExecuteProcedure(string json)
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Not connected");
            }
            process.StandardInput.Write(json);
            process.StandardInput.Write('\0');
            return process.StandardOutput.ReadLine('\0');
        }

        public LocalSolverConnector(JObject jo): base(jo)
        {
        }

        public override void Connect()
        {
            if (Connected)
            {
                throw new InvalidOperationException("Already connected");
            }
            process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = settings["solver"]["filename"].ToString();
            process.Start();
            Connected = true;
            Handshake();
        }

        public override void Disconnect()
        {
            if (!Connected)
            {
                return;
            }
            process.StandardInput.Close();
            new Thread(() =>
            {
                Thread.Sleep(1000);
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }).Start();
            Connected = false;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    process.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    static class StreamReaderReadLine
    {
        public static string ReadLine(this StreamReader reader, char delimiter)
        {
            List<char> chars = new List<char>();
            while (!reader.EndOfStream)
            {

                char c = (char)reader.Read();

                if (c == delimiter)
                {
                    break;
                }

                chars.Add(c);
            }
            return new String(chars.ToArray());
        }
    }

}
