using Scribe.Core.ConnectorApi;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Event : BaseObject
    {

        public Event(string accountId, string eventId) : base(accountId, eventId,
            Constants.Event_Name, Constants.Event_FullName, Constants.Event_Description)
        {
            setPropertyDefinitions();
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
                    var ds = DataServicesClient.ListSpeakers(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
                    var table = ds.Tables["ResultSet"];
                    var filteredRows = table.Select($"{Constants.Speaker_PK} = {de.Properties[Constants.Speaker_PK]}");
                    List<DataEntity> children = new List<DataEntity>();
                    foreach (var c in filteredRows.ToDataEntities(Name))
                        children.Add(c);
                    de.Children.Add(Constants.Speaker_Name, children);
                }
            }
        }


    }
}
