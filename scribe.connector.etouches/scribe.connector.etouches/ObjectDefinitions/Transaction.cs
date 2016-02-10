using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Transaction : ObjectDefinition
    {
        private string eventId;

        public Transaction(string eventId)
        {
            this.eventId = eventId;

            FullName = "Transaction";
            Description = "A single Transaction";
            Hidden = false;
            Name = "Transaction";
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
                Description = "Transaction Id",
                FullName = "Transaction Id",
                IsPrimaryKey = true,
                MaxOccurs = 1,
                MinOccurs = 0,
                Name = "TransactionId",
                PresentationType = "string",
                PropertyType = typeof(string).Name,
                UsedInQuerySelect = true
            });

        }
    }
}
