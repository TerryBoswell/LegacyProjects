using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Session : BaseObject
    {
        private string eventId;

        public Session(string accountId, string eventId): base(accountId, eventId)
        {
            this.eventId = eventId;
            FullName = "Session";
            Description = "A single Session";
            Hidden = false;
            Name = "Session";
            SupportedActionFullNames = new List<string> { "Query" };
            setPropertyDefinitions();
        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetSessionMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            base.SetPropertyDefinitions(data);
        }

        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            var ds = DataServicesClient.ListSessions(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            var table = ds.Tables["ResultSet"];

            var filteredRows = table.Select(query.ToSelectExpression());
            return filteredRows.ToDataEntities(query.RootEntity.ObjectDefinitionFullName);
        }
    }
}
