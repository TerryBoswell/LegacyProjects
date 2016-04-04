using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Connector.etouches
{
    public static class Logger
    {
        public static void WriteError(string msg)
        {
            Write(msg, Core.ConnectorApi.Logger.Logger.Severity.Error);
        }

        public static void WriteDebug(string msg)
        {
            Write(msg, Core.ConnectorApi.Logger.Logger.Severity.Debug);
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
            catch { }
        }
    }
}
