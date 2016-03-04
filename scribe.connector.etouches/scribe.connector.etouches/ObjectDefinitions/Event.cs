using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    /// <summary>
    /// Event
    /// Children - Speaker, Session, Meeting
    /// Parents - None
    /// </summary>
    class Event : BaseObject
    {

        public Event(string accountId, string eventId) : base(accountId, eventId,
            Constants.Event_Name, Constants.Event_FullName, Constants.Event_Description)
        {
            RelationshipDefinitions = getRelationshipDefinitions();
            setPropertyDefinitions();
        }

        private List<IRelationshipDefinition> getRelationshipDefinitions()
        {
            //Events Children => Speaker, Session, Meeting
            var relationships = new List<IRelationshipDefinition>();
            relationships.Add(new RelationshipDefinition()
            {
                Description = string.Empty,
                Name = Constants.BuildChildRelationship(Constants.Speaker_Name, this.Name),
                FullName = Constants.Speaker_FullName,
                RelationshipType = RelationshipType.Child,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Speaker_PK,
                RelatedObjectDefinitionFullName = Constants.Speaker_FullName,
                RelatedProperties = Constants.Speaker_PK
            });
            relationships.Add(new RelationshipDefinition()
            {
                Description = string.Empty,
                Name = Constants.BuildChildRelationship(Constants.Session_Name, this.Name),
                FullName = Constants.Session_FullName,
                RelationshipType = RelationshipType.Child,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Session_PK,
                RelatedObjectDefinitionFullName = Constants.Session_FullName,
                RelatedProperties = Constants.Session_PK
            });
            relationships.Add(new RelationshipDefinition()
            {
                Description = string.Empty,
                Name = Constants.BuildChildRelationship(Constants.Meeting_Name, this.Name),
                FullName = Constants.Meeting_FullName,
                RelationshipType = RelationshipType.Child,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Meeting_PK,
                RelatedObjectDefinitionFullName = Constants.Meeting_FullName,
                RelatedProperties = Constants.Meeting_PK
            });
            return relationships;

        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetEventMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId);
            base.SetPropertyDefinitions(data);
        }


        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            this.SetQuery(query);
            var ds = DataServicesClient.ListEvents(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.ModifiedAfterDate, this.AttendeeModifiedAfterDate);
            var table = ds.Tables["ResultSet"];
            var filteredRows = table.Select(query.ToSelectExpression());
            var dataEntities = filteredRows.ToDataEntities(query.RootEntity.ObjectDefinitionFullName);
            PopulateChildData(dataEntities);
            return dataEntities;
        }

        internal void PopulateChildData(IEnumerable<DataEntity> dataEntities)
        {
            if (!this.HasChildren)
                return;
            foreach (var de in dataEntities)
            {
                de.Children = new Core.ConnectorApi.Query.EntityChildren();
                //Events Children => Speaker, Session, Meeting
                if (this.ChildNames.Any(x => x.Equals(Constants.Speaker_Name)))
                {
                    var ds = DataServicesClient.ListSpeakers(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Event_PK} = {de.Properties[Constants.Event_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Constants.Speaker_Name))
                        children.Add(c);
                    de.Children.Add(Constants.Speaker_Name, children);
                }
                if (this.ChildNames.Any(x => x.Equals(Constants.Session_Name)))
                {
                    var ds = DataServicesClient.ListSessions(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Event_PK} = {de.Properties[Constants.Event_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Constants.Session_Name))
                        children.Add(c);
                    de.Children.Add(Constants.Session_Name, children);
                }
                if (this.ChildNames.Any(x => x.Equals(Constants.Meeting_Name)))
                {
                    var ds = DataServicesClient.ListMeetings(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Event_PK} = {de.Properties[Constants.Event_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Constants.Meeting_Name))
                        children.Add(c);
                    de.Children.Add(Constants.Meeting_Name, children);
                }
            }
        }


    }
}
