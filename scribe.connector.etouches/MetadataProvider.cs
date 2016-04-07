using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Connector.etouches
{
    class MetadataProvider : IMetadataProvider
    {

        private readonly ScribeConnection connection;
        
        public MetadataProvider(ScribeConnection connection)
        {
            this.connection = connection;
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
            var returnSet = new List<IObjectDefinition>
            {
                new ObjectDefinitions.Event(connection),
                new ObjectDefinitions.Attendee(connection),
                new ObjectDefinitions.RegSession(connection),
                new ObjectDefinitions.Speaker(connection),
                new ObjectDefinitions.Session(connection),
                new ObjectDefinitions.Meeting(connection),
                new ObjectDefinitions.SessionTrack(connection)
                //new ObjectDefinitions.Category(eventId),
                //new ObjectDefinitions.Hotel(eventId),
                //new ObjectDefinitions.Invoice(eventId),
                //new ObjectDefinitions.Question(eventId),
                //new ObjectDefinitions.Report(eventId),
                //new ObjectDefinitions.RoomType(eventId),
                //new ObjectDefinitions.Transaction(eventId),                
            };

            if (Configuration.FinancialTransactionsEnabled)
                returnSet.Add(new ObjectDefinitions.FinancialTransaction(connection));

            return returnSet;
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
                    SupportsRelations = true,
                }
            };
        }


    }
}
