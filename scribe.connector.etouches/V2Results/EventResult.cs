using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Connector.etouches.V2Results
{
    public class EventResult
    {
        public EventResult() { }

        public int EventId { get; set; }
        public string Description { get; set; }

        private bool hasError;

        public bool HasError
        {
            get { return hasError; }
        }
        public object Error {
            set
            {
                if (value != null)
                    hasError = true;
            }
        }

        
    }


}
