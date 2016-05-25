using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    /// <summary>
    /// Event
    /// Children - Speaker, Session, Meeting, Financial Transaction
    /// Parents - None
    /// </summary>
    class Event : BaseObject
    {
        public Event(ScribeConnection connection) : base(connection,
            Constants.Event_Name, Constants.Event_FullName, Constants.Event_Description, 
            Constants.QueryAction.Query, Constants.QueryAction.Create)
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
            relationships.Add(new RelationshipDefinition()
            {
                Description = string.Empty,
                Name = Constants.BuildChildRelationship(Constants.FinancialTranstion_Name, this.Name),
                FullName = Constants.FinancialTranstion_FullName,
                RelationshipType = RelationshipType.Child,
                ThisObjectDefinitionFullName = this.FullName,
                ThisProperties = Constants.FinancialTranstion_PK,
                RelatedObjectDefinitionFullName = Constants.FinancialTranstion_FullName,
                RelatedProperties = Constants.FinancialTranstion_PK
            });
            return relationships;

        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetEventMetaData(Connection);
            base.SetPropertyDefinitions(data);
        }


        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            this.SetQuery(query);
            var ds = DataServicesClient.ListEvents(Connection, this.ModifiedAfterDate, null, null, this.KeyPairs);
            var dataEntities = GetDataEntites(ds, query);
            PopulateChildData(dataEntities);
            return dataEntities;
        }

        internal MethodResult Create(DataEntity dataEntity, ScribeConnection v2Connection)
        {
            if (!dataEntity.Properties.ContainsKey(Constants.Event_NameProperty))
                throw new System.Exception("The data enitity is missing the name property");
            var name = dataEntity.Properties[Constants.Event_NameProperty].ToString();
            var result = DataServicesClient.CreateEvent(v2Connection, name);

            if (result.HasError)
                return new MethodResult() {Success = false, ErrorInfo = new ErrorResult() {
                    Description = result.Error.ToString()
                } };

            return updateEvent(result.EventId, dataEntity);
        }

        private MethodResult updateEvent(int eventId, DataEntity entity)
        {
            var name = entity.GetProperty("eventname");
            var code = entity.GetProperty("code");
            var startDate = entity.GetDateProperty("starttime");
            var endDate = entity.GetDateProperty("enddate");
            var locationName = entity.GetProperty("locationname");
            var programManager = entity.GetProperty("programmanager");
            var max_reg = entity.GetProperty("max_reg");
            var status = entity.GetProperty("eventstatus");
            var result = DataServicesClient.UpdateEvent(Connection, eventId, name, code, startDate,
                endDate, locationName, programManager, status, max_reg);

            if (result.HasError)
                return new MethodResult()
                {
                    Success = false,
                    ErrorInfo = new ErrorResult()
                    {
                        Description = result.Error.ToString()
                    }
                };

            return new MethodResult() { Success = !result.HasError };
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
                    var ds = DataServicesClient.ListSpeakers(Connection);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Event_PK} = {de.Properties[Constants.Event_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Constants.Speaker_Name, this.PropertyDefinitions))
                        children.Add(c);
                    de.Children.Add(Constants.Speaker_Name, children);
                }
                if (this.ChildNames.Any(x => x.Equals(Constants.Session_Name)))
                {
                    var ds = DataServicesClient.ListSessions(Connection);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Event_PK} = {de.Properties[Constants.Event_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Constants.Session_Name, this.PropertyDefinitions))
                        children.Add(c);
                    de.Children.Add(Constants.Session_Name, children);
                }
                if (this.ChildNames.Any(x => x.Equals(Constants.Meeting_Name)))
                {
                    var ds = DataServicesClient.ListMeetings(Connection);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Event_PK} = {de.Properties[Constants.Event_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Constants.Meeting_Name, this.PropertyDefinitions))
                        children.Add(c);
                    de.Children.Add(Constants.Meeting_Name, children);
                }
                if (this.ChildNames.Any(x => x.Equals(Constants.FinancialTranstion_Name)))
                {
                    var ds = DataServicesClient.ListFinancialTransactions(Connection);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Event_PK} = {de.Properties[Constants.Event_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Constants.FinancialTranstion_Name, this.PropertyDefinitions))
                        children.Add(c);
                    de.Children.Add(Constants.FinancialTranstion_Name, children);
                }

            }
        }


    }
}
