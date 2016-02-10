using Newtonsoft.Json.Linq;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class RegSession : BaseObject
    {

        public RegSession(string accountId, string eventId) : base(accountId, eventId)
        {

            FullName = "RegSession";
            Description = "RegSession";
            Hidden = false;
            Name = "RegSession";
            SupportedActionFullNames = new List<string> { "Query" };

            setPropertyDefinitions();
        }


        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetRegSessionMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            base.SetPropertyDefinitions(data);
        }


        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            var ds = DataServicesClient.ListRegSessions(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            var table = ds.Tables["ResultSet"];

            var filteredRows = table.Select(query.ToSelectExpression());
            return filteredRows.ToDataEntities(query.RootEntity.ObjectDefinitionFullName);
        }

    }
}
