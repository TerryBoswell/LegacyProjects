using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class RoomType : ObjectDefinition
    {
        private string eventId;

        public RoomType(string eventId)
        {
            this.eventId = eventId;

            FullName = "Room Type";
            Description = "A single Room Type";
            Hidden = false;
            Name = "RoomType";
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
                Description = "RoomType Id",
                FullName = "RoomType Id",
                IsPrimaryKey = true,
                MaxOccurs = 1,
                MinOccurs = 0,
                Name = "RoomTypeId",
                PresentationType = "string",
                PropertyType = typeof(string).Name,
                UsedInQuerySelect = true
            });

        }
    }
}
