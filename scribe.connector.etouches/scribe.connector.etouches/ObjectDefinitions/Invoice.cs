using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Invoice : ObjectDefinition
    {
        private string eventId;

        public Invoice(string eventId)
        {
            this.eventId = eventId;

            FullName = "Invoice";
            Description = "An Attendee's Invoice";
            Hidden = false;
            Name = "Invoice";
            SupportedActionFullNames = new List<string> { "Query" };

            setPropertyDefinitions();
        }

        private void setPropertyDefinitions()
        {
            //var ev = ApiV2Client.GetEvent(Connector.BaseUrl, Connector.AccessToken, Connector.EventId);
            PropertyDefinitions = new List<IPropertyDefinition>();
            
            //add eventid as PK
            PropertyDefinitions.Add(new PropertyDefinition
            {
                Description = "Invoice Id",
                FullName = "Invoice Id",
                IsPrimaryKey = true,
                MaxOccurs = 1,
                MinOccurs = 0,
                Name = "InvoiceId",
                PresentationType = "string",
                PropertyType = typeof(string).Name,
                UsedInQuerySelect = true
            });

        }
    }
}
