using Scribe.Core.ConnectorApi;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi.Metadata;
using System;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    /// <summary>
    /// Attendee
    /// Parents :
    /// Children : RegSessions
    /// </summary>
    class Attendee : BaseObject
    {

        public Attendee(ScribeConnection connection) : base(connection, Constants.Attendee_FullName, 
            Constants.Attendee_Name, Constants.Attendee_Description )
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
                Name = Constants.BuildChildRelationship(Constants.RegSession_Name, this.Name),
                FullName = Constants.RegSession_Name,//Name,
                RelationshipType = RelationshipType.Child,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.Attendee_PK,
                RelatedObjectDefinitionFullName = Constants.RegSession_FullName,
                RelatedProperties = Constants.Attendee_PK
            });

            return relationships;

        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetAttendeeMetaData(Connection);
            base.SetPropertyDefinitions(data);
        }

        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            this.SetQuery(query);
            var ds = DataServicesClient.ListAttendees(this.Connection, this.ModifiedAfterDate, null, this.KeyPairs);
            var dataEntities = GetDataEntites(ds, query);
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
                //Handle the RegSession Relationship
                if (this.ChildNames.Any(x => x.Equals(Constants.RegSession_Name)))
                {
                    var ds = DataServicesClient.ListRegSessions(Connection);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Attendee_PK} = {de.Properties[Constants.Attendee_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (
                        var c in filteredRows.ToDataEntities(Constants.RegSession_Name))
                        children.Add(c);
                    de.Children.Add(Constants.RegSession_Name, children);
                }
            }
        }

    }
}
