using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    /// <summary>
    /// Meeting 
    /// Children  - None
    /// Parents - Events, Attendee
    /// </summary>
    class FinancialTransaction : BaseObject
    {

        public FinancialTransaction(ScribeConnection connection) : base(connection,
            Constants.FinancialTranstion_Name, Constants.FinancialTranstion_FullName, Constants.FinancialTranstion_Description)
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
                Name = Constants.BuildParentRelationship(this.Name, Constants.Attendee_Name),
                FullName = Constants.Attendee_FullName,
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
            var data = DataServicesClient.GetFinancialTransactionMetaData(Connection);
            base.SetPropertyDefinitions(data);
        }


        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            this.SetQuery(query);
            var ds = DataServicesClient.ListFinacialTransactions(Connection, this.KeyPairs);
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
                if (this.ChildNames.Any(x => x.Equals(Constants.Event_Name)))
                {
                    var ds = DataServicesClient.ListEvents(Connection);
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

                if (this.ChildNames.Any(x => x.Equals(Constants.Attendee_Name)))
                {
                    var ds = DataServicesClient.ListEvents(Connection);
                    var table = ds.Tables["ResultSet"];

                    var filteredRows = table.Select($"{Constants.Attendee_PK} = {de.Properties[Constants.Attendee_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    var parent = filteredRows.FirstDataEntity(Constants.Attendee_Name);
                    if (parent != null)
                    {
                        children.Add(parent);
                        de.Children.Add(Constants.Attendee_Name, children);
                    }
                }

            }
        }



    }
}
