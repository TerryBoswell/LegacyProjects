using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Connector.etouches
{
    class MetadataProvider : IMetadataProvider
    {
        private string eventId;
        private string accountId;
        
        public MetadataProvider(string accountId, string eventId)
        {
            this.eventId = eventId;
            this.accountId = accountId;
        }

        public void ResetMetadata()
        {
            this.objectDefinitions = getObjectDefintions();
        }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            return this.ActionDefinitions;
        }

        public IMethodDefinition RetrieveMethodDefinition(string objectName, bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public IObjectDefinition RetrieveObjectDefinition(string objectName, bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            return this.ObjectDefinitions.First(x => x.FullName == objectName);
        }

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            return this.ObjectDefinitions;
        }

        public void Dispose()
        {
        }

        private IEnumerable<IObjectDefinition> objectDefinitions;
        private IEnumerable<IActionDefinition> actionDefinitions;

        public IEnumerable<IObjectDefinition> ObjectDefinitions { get { return this.objectDefinitions ?? (this.objectDefinitions = getObjectDefintions()); } }
        public IEnumerable<IActionDefinition> ActionDefinitions { get { return this.actionDefinitions ?? (this.actionDefinitions = getActionDefintions()); } }

        private IEnumerable<IObjectDefinition> getObjectDefintions()
        {
            return new List<IObjectDefinition>
            {
                new ObjectDefinitions.Event(accountId, eventId),
                new ObjectDefinitions.Attendee(accountId, eventId),
                new ObjectDefinitions.RegSession(accountId, eventId),
                new ObjectDefinitions.Speaker(accountId, eventId),
                new ObjectDefinitions.Session(accountId, eventId),
                new ObjectDefinitions.Meeting(accountId, eventId)                
                //new ObjectDefinitions.Category(eventId),
                //new ObjectDefinitions.Hotel(eventId),
                //new ObjectDefinitions.Invoice(eventId),
                //new ObjectDefinitions.Question(eventId),
                //new ObjectDefinitions.Report(eventId),
                //new ObjectDefinitions.RoomType(eventId),
                //new ObjectDefinitions.Transaction(eventId),                
            };
        }

        private IEnumerable<IActionDefinition> getActionDefintions()
        {
            return new List<IActionDefinition>
            {
                new ActionDefinition
                {
                    Description = "List Entities",
                    FullName = "Query",
                    Name = "Query",
                    KnownActionType = KnownActions.Query,
                    SupportsConstraints = true,

                }
            };
        }


    }
}
