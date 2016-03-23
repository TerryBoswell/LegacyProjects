using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    /// <summary>
    /// Meeting 
    /// Children  - None
    /// Parents - Events
    /// </summary>
    class Meeting : BaseObject
    {

        public Meeting(string accountId, string eventId) : base(accountId, eventId,
            Constants.Meeting_Name, Constants.Meeting_FullName, Constants.Meeting_Description)
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

            return relationships;

        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetMeetingMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            base.SetPropertyDefinitions(data);
        }


        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            this.SetQuery(query);
            var ds = DataServicesClient.ListMeetings(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId, this.KeyPairs);
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
                if (this.ChildNames.Any(x => x.Equals(Constants.RegSession_Name)))
                {
                    var ds = DataServicesClient.ListEvents(Connector.BaseUrl, Connector.AccessToken, this.AccountId);
                    var table = ds.Tables["ResultSet"];
                  
                    var filteredRows = table.Select($"{Constants.Event_PK} = {de.Properties[Constants.Event_PK]}");
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
