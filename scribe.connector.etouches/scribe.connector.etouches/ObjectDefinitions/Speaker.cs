using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Speaker : BaseObject
    {
        public Speaker(string accountId, string eventId) : base(accountId, eventId)
        {
            FullName = "Speaker";
            Description = "A single Speaker";
            Hidden = false;
            Name = "Speaker";
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
            var ds = DataServicesClient.ListSpeakers(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            var table = ds.Tables["ResultSet"];

            var filteredRows = table.Select(query.ToSelectExpression());
            return filteredRows.ToDataEntities(query.RootEntity.ObjectDefinitionFullName);
        }
    }
}
