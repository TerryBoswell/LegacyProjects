using Newtonsoft.Json.Linq;
using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    abstract class BaseObject : ObjectDefinition
    {
        protected string EventId;
        protected string AccountId;

        public BaseObject(string accountId, string eventId)
        {
            this.EventId = eventId;
            this.AccountId = accountId;
        }

        virtual protected void SetPropertyDefinitions(JObject o)
        {
            PropertyDefinitions = new List<IPropertyDefinition>();

            //populate the rest of the properties
            JArray fields = (JArray)o["fields"];

            foreach (JObject field in fields)
            {
                PropertyDefinitions.Add(new PropertyDefinition
                {  //TODO: Fill these in from the metadata results as soon as the API support them
                    Description = (string)field["description"],
                    FullName = (string)field["fullName"],
                    IsPrimaryKey = ((string)field["isPrimaryKey"] == "yes"),
                    Nullable = ((string)field["nullable"] == "yes"),
                    MaxOccurs = 1,
                    MinOccurs = 0,
                    Name = (string)field["fullName"],
                    PresentationType = (string)field["presentationType"],
                    PropertyType = (string)field["presentationType"],
                    UsedInQuerySelect = true,
                    UsedInLookupCondition = true,
                    UsedInQueryConstraint = true
                });

            }
        }

    }
}
