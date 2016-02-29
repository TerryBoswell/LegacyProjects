using Newtonsoft.Json.Linq;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class RegSession : BaseObject
    {

        public RegSession(string accountId, string eventId) : base(accountId, eventId,
            Constants.RegSession_Name, Constants.RegSession_FullName, Constants.RegSession_Description)
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
                Name = Constants.BuildParentRelationship(Constants.RegSession_Name, Constants.Attendee_Name),
                FullName = this.FullName,
                RelationshipType = RelationshipType.Parent,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Attendee_PK,
                RelatedObjectDefinitionFullName = Constants.Attendee_FullName,
                RelatedProperties = Constants.Attendee_PK
            });

            return relationships;

        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetRegSessionMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            base.SetPropertyDefinitions(data);
        }


        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            this.SetQuery(query);
            var ds = DataServicesClient.ListRegSessions(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
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
                if (this.ChildNames.Any(x => x.Equals(this.Name)))
                {
                    var ds = DataServicesClient.ListRegSessions(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Attendee_PK} = {de.Properties[Constants.Attendee_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Constants.RegSession_Name))
                        children.Add(c);
                    de.Children.Add(Constants.RegSession_Name, children);
                }
            }
        }

    }
}
