using Scribe.Core.ConnectorApi;
using System.Collections.Generic;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Event : BaseObject
    {

        public Event(string accountId, string eventId) : base(accountId, eventId)
        {

            FullName = "Event";
            Description = "A single Event";
            Hidden = false;
            Name = "Event";
            SupportedActionFullNames = new List<string> { "Query" };
            setPropertyDefinitions();
        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetEventMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId);
            base.SetPropertyDefinitions(data);
        }


        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            var ds = DataServicesClient.ListEvents(Connector.BaseUrl, Connector.AccessToken, this.AccountId);
            var table = ds.Tables["ResultSet"];
            var filteredRows = table.Select(query.ToSelectExpression());
            return filteredRows.ToDataEntities(query.RootEntity.ObjectDefinitionFullName);
        }    


        

    }
}
