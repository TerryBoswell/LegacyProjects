using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Connector.etouches
{
    public class DiagnosticsLog : IDisposable
    {
        private System.Diagnostics.Stopwatch stopWatch;
        public string Name;
        public double TotalSeconds;
        public DiagnosticsLog(string actionName)
        {
            stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            this.Name = actionName;
        }
        public void Dispose()
        {
            this.stopWatch.Stop();
            this.TotalSeconds = this.stopWatch.Elapsed.TotalSeconds;
            Logger.AddDiagnosticsLog(this);

        }
    }
}
