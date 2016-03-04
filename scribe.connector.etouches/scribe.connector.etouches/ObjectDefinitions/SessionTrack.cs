using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class SessionTrack : BaseObject
    {
        /// <summary>
        /// Session Track
        /// Parents: Session
        /// Children: 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="eventId"></param>
        public SessionTrack(string accountId, string eventId): base(accountId, eventId,
            Constants.SessionTrack_Name, Constants.SessionTrack_FullName, Constants.SessionTrack_Description)
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
                Name = Constants.BuildParentRelationship(this.Name, Constants.Session_Name),
                FullName = Constants.Session_FullName,
                RelationshipType = RelationshipType.Parent,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Session_PK,
                RelatedObjectDefinitionFullName = Constants.Session_FullName,
                RelatedProperties = Constants.Session_tempPk
            });
                    
            return relationships;

        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetSessionTrackMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            base.SetPropertyDefinitions(data);
        }

        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            this.SetQuery(query);

            var ds = DataServicesClient.ListSessionTracks(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            var table = ds.Tables["ResultSet"];
            var filteredRows = table.Select(query.ToSelectExpression());
            var dataEntities = filteredRows.ToDataEntities(query.RootEntity.ObjectDefinitionFullName);
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
                if (this.ChildNames.Any(x => x.Equals(Constants.Session_Name)))
                {
                    var ds = DataServicesClient.ListSessions(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
                    var table = ds.Tables["ResultSet"];

                    var filteredRows = table.Select($"{Constants.Session_tempPk} = '{de.Properties[Constants.Session_PK]}'");
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
