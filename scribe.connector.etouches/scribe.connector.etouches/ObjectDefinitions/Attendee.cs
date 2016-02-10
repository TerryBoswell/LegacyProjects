using Scribe.Core.ConnectorApi;
using System.Collections.Generic;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Attendee : BaseObject
    {

        public Attendee(string accountId, string eventId) : base(accountId, eventId)
        {

            FullName = "Attendee";
            Description = "Attendee";
            Hidden = false;
            Name = "Attendee";
            SupportedActionFullNames = new List<string> { "Query" };

            setPropertyDefinitions();
        }


        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetAttendeeMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            base.SetPropertyDefinitions(data);
        }

        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            var ds = DataServicesClient.ListAttendees(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            var table = ds.Tables["ResultSet"];

            var filteredRows = table.Select(query.ToSelectExpression());
            return filteredRows.ToDataEntities(query.RootEntity.ObjectDefinitionFullName);
        }    

    }
}
