using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Session : BaseObject
    {
        
        public Session(string accountId, string eventId): base(accountId, eventId,
            Constants.Session_Name, Constants.Session_FullName, Constants.Session_Description)
        {
           setPropertyDefinitions();
        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetSessionMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            base.SetPropertyDefinitions(data);
        }

        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            this.SetQuery(query);

            var ds = DataServicesClient.ListSessions(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            var table = ds.Tables["ResultSet"];
            var filteredRows = table.Select(query.ToSelectExpression());
            return filteredRows.ToDataEntities(query.RootEntity.ObjectDefinitionFullName);
        }
    }
}
