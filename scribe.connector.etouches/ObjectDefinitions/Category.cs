using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Category : ObjectDefinition
    {
        private string eventId;

        public Category(string eventId)
        {
            this.eventId = eventId;

            FullName = "Category";
            Description = "A single Category";
            Hidden = false;
            Name = "Category";
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
                Description = "Category Id",
                FullName = "Category Id",
                IsPrimaryKey = true,
                MaxOccurs = 1,
                MinOccurs = 0,
                Name = "CategoryId",
                PresentationType = "string",
                PropertyType = typeof(string).Name,
                UsedInQuerySelect = true
            });

        }
    }
}
