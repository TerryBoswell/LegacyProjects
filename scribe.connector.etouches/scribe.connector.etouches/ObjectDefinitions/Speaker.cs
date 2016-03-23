using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Speaker : BaseObject
    {
        /// <summary>
        /// Speaker
        /// Parents: Event, session
        /// Children:
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="eventId"></param>
        public Speaker(ScribeConnection connection) : base(connection,
            Constants.Speaker_Name, Constants.Speaker_FullName, Constants.Speaker_Description)
        {
            RelationshipDefinitions = getRelationshipDefinitions();
            setPropertyDefinitions();
        }

        private List<IRelationshipDefinition> getRelationshipDefinitions()
        {
            var relationships = new List<IRelationshipDefinition>();
            relationships.Add(new RelationshipDefinition()
            {
                Description = string.Empty,
                Name = Constants.BuildParentRelationship(this.Name, Constants.Event_Name),
                FullName = Constants.Event_FullName,
                RelationshipType = RelationshipType.Parent,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Event_PK,
                RelatedObjectDefinitionFullName = Constants.Event_FullName,
                RelatedProperties = Constants.Event_PK
            });

            relationships.Add(new RelationshipDefinition()
            {
                Description = string.Empty,
                Name = Constants.BuildParentRelationship(this.Name, Constants.Session_Name),
                FullName = Constants.Session_FullName,
                RelationshipType = RelationshipType.Parent,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Session_PK,
                RelatedObjectDefinitionFullName = Constants.Session_FullName,
                RelatedProperties = Constants.Session_PK
            });

            return relationships;

        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetRegSessionMetaData(Connection.BaseUrl, Connection.AccessToken, Connection.AccountId, Connection.EventId);
            base.SetPropertyDefinitions(data);
        }


        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            this.SetQuery(query);
            var ds = DataServicesClient.ListSpeakers(Connection, this.KeyPairs);
            var dataEntities = GetDataEntites(ds, query);
            PopulateParentData(dataEntities);
            return dataEntities;
        }

        internal void PopulateParentData(IEnumerable<DataEntity> dataEntities)
        {
            if (!this.HasChildren)
                return;
            foreach (var de in dataEntities)
            {
                de.Children = new Core.ConnectorApi.Query.EntityChildren();
                //Event, session
                if (this.ChildNames.Any(x => x.Equals(Constants.Event_Name)))
                {
                    var ds = DataServicesClient.ListEvents(Connection);
                    var table = ds.Tables["ResultSet"];

                    var filteredRows = table.Select($"{Constants.Event_PK} = '{de.Properties[Constants.Event_PK]}'");
                    List<DataEntity> children = new List<DataEntity>();
                    var parent = filteredRows.FirstDataEntity(Constants.Event_Name);
                    if (parent != null)
                    {
                        children.Add(parent);
                        de.Children.Add(Constants.Event_Name, children);
                    }
                }

                if (this.ChildNames.Any(x => x.Equals(Constants.Session_Name)))
                {
                    var ds = DataServicesClient.ListSessions(Connection);
                    var table = ds.Tables["ResultSet"];

                    var filteredRows = table.Select($"{Constants.Session_PK} = {de.Properties[Constants.Session_tempPk]}");
                    List<DataEntity> children = new List<DataEntity>();
                    var parent = filteredRows.FirstDataEntity(Constants.Session_Name);
                    if (parent != null)
                    {
                        children.Add(parent);
                        de.Children.Add(Constants.Session_Name, children);
                    }
                }
            }
        }
    }
}
