using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Hotel : ObjectDefinition
    {
        private string eventId;

        public Hotel(string eventId)
        {
            this.eventId = eventId;

            FullName = "Hotel";
            Description = "A single Hotel";
            Hidden = false;
            Name = "Hotel";
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
                Description = "Hotel Id",
                FullName = "Hotel Id",
                IsPrimaryKey = true,
                MaxOccurs = 1,
                MinOccurs = 0,
                Name = "HotelId",
                PresentationType = "string",
                PropertyType = typeof(string).Name,
                UsedInQuerySelect = true
            });

        }
    }
}
