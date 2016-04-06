using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Connector.etouches
{
    public static class Logger
    {
        private static bool? diagnostics;
        public static bool Diagnostics
        {
            get
            {
                if (!diagnostics.HasValue)
                    diagnostics = bool.Parse(System.Configuration.ConfigurationSettings.AppSettings["Diagnostics"]);
                return diagnostics.Value;
            }
        }

        private static System.Collections.Concurrent.ConcurrentDictionary<string, double> logs = new System.Collections.Concurrent.ConcurrentDictionary<string, double>();
        public static void AddDiagnosticsLog(DiagnosticsLog log)
        {
            logs.AddOrUpdate(log.Name, log.TotalSeconds, (s, i) =>
            {
                return logs[s] + i;
            });
        }

        public static void WriteError(string msg)
        {
            Write(msg, Core.ConnectorApi.Logger.Logger.Severity.Error);
        }

        public static void WriteDebug(string msg)
        {
            var elapsed = logs.GroupBy(x => x.Key).Select(l => new {
                Name = l.Key,
                Elapsed = l.Sum(x => x.Value)
            });

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(msg);
            foreach (var kp in elapsed)
            {
                sb.AppendLine($"Action: {kp.Name} : Total Seconds : {kp.Elapsed}");
            }
           
            Write(sb.ToString(), Core.ConnectorApi.Logger.Logger.Severity.Debug);
            System.Diagnostics.Debug.WriteLine(msg);
        }

        public static void WriteInfo(string msg)
        {
            Write(msg, Core.ConnectorApi.Logger.Logger.Severity.Info);
        }

        public static void WriteWarning(string msg)
        {
            Write(msg, Core.ConnectorApi.Logger.Logger.Severity.Warning);
        }

        private static void Write(string msg, Core.ConnectorApi.Logger.Logger.Severity severity)
        {
            try
            {
                msg = $"eTouches:{msg}";
                Core.ConnectorApi.Logger.Logger.Write(severity, ObjectDefinitions.Constants.ConnectorTitle, msg);
            }
            catch(Exception ex) {
                
                
            }
        }
    }
}
