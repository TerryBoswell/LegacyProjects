using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    /// <summary>
    /// Session
    /// Children -> RegSession, SessionTrack
    /// Parent -> Event
    /// </summary>
    class Session : BaseObject
    {
        
        public Session(string accountId, string eventId): base(accountId, eventId,
            Constants.Session_Name, Constants.Session_FullName, Constants.Session_Description)
        {
            RelationshipDefinitions = getRelationshipDefinitions();
            setPropertyDefinitions();
        }

        private List<IRelationshipDefinition> getRelationshipDefinitions()
        {
            var relationships = new List<IRelationshipDefinition>();
            //Parents
            relationships.Add(new RelationshipDefinition()
            {
                Description = string.Empty,
                Name = Constants.BuildParentRelationship(this.Name, Constants.Event_Name),
                FullName = Constants.Event_Name,
                RelationshipType = RelationshipType.Parent,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Event_PK,
                RelatedObjectDefinitionFullName = Constants.Event_FullName,
                RelatedProperties = Constants.Event_PK
            });
            //children  => RegSession, SessionTrack
            relationships.Add(new RelationshipDefinition()
            {
                Description = string.Empty,
                Name = Constants.BuildChildRelationship(Constants.RegSession_Name, this.Name),
                FullName = Constants.RegSession_Name,
                RelationshipType = RelationshipType.Child,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Session_PK, //Constants.Session_PK,
                RelatedObjectDefinitionFullName = Constants.RegSession_FullName,
                RelatedProperties = Constants.Session_PK
            });

            relationships.Add(new RelationshipDefinition()
            {
                Description = string.Empty,
                Name = Constants.BuildChildRelationship(Constants.SessionTrack_Name, this.Name),
                FullName = Constants.SessionTrack_FullName,
                RelationshipType = RelationshipType.Child,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Session_PK, // Constants.Session_PK,
                RelatedObjectDefinitionFullName = Constants.SessionTrack_FullName,
                RelatedProperties = Constants.Session_PK
            });

            return relationships;

        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetSessionMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            base.SetPropertyDefinitions(data);
            
        }

        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            this.SetQuery(query);

            var ds = DataServicesClient.ListSessions(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId, this.KeyPairs);
            var dataEntities = GetDataEntites(ds, query);
            PopulateChildData(dataEntities);
            PopulateParentData(dataEntities); 
            return dataEntities;
        }

        internal void PopulateChildData(IEnumerable<DataEntity> dataEntities)
        {
            if (!this.HasChildren)
                return;


            foreach (var de in dataEntities)
            {
                de.Children = new Core.ConnectorApi.Query.EntityChildren();
                if (this.ChildNames.Any(x => x.Equals(Constants.RegSession_Name)))
                {
                    var ds = DataServicesClient.ListRegSessions(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Session_PK} = {de.Properties[Constants.Session_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Constants.RegSession_Name))
                        children.Add(c);
                    de.Children.Add(Constants.RegSession_Name, children);
                }
                if (this.ChildNames.Any(x => x.Equals(Constants.SessionTrack_Name)))
                {
                    var ds = DataServicesClient.ListSessionTracks(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Session_PK} = {de.Properties[Constants.Session_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Constants.SessionTrack_Name))
                        children.Add(c);
                    de.Children.Add(Constants.SessionTrack_Name, children);
                }
            }
        }

        internal void PopulateParentData(IEnumerable<DataEntity> dataEntities)
        {
            if (!this.HasChildren)
                return;
            foreach (var de in dataEntities)
            {
                de.Children = new Core.ConnectorApi.Query.EntityChildren();
                if (this.ChildNames.Any(x => x.Equals(Constants.Event_Name)))
                {
                    var ds = DataServicesClient.ListEvents(Connector.BaseUrl, Connector.AccessToken, this.AccountId);
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

            }
        }

    }
}
