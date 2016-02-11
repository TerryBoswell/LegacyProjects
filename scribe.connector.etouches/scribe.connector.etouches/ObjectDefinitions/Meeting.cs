﻿using Scribe.Core.ConnectorApi;
using System.Collections.Generic;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    class Meeting : BaseObject
    {

        public Meeting(string accountId, string eventId) : base(accountId, eventId)
        {

            FullName = "Meeting";
            Description = "eSocial meeting for an event";
            Hidden = false;
            Name = "Meeting";
            SupportedActionFullNames = new List<string> { "Query" };
            setPropertyDefinitions();
        }

        private void setPropertyDefinitions()
        {
            var data = DataServicesClient.GetMeetingMetaData(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            base.SetPropertyDefinitions(data);
        }


        internal IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {
            var ds = DataServicesClient.ListMeetings(Connector.BaseUrl, Connector.AccessToken, this.AccountId, this.EventId);
            var table = ds.Tables["ResultSet"];
            var filteredRows = table.Select(query.ToSelectExpression());
            return filteredRows.ToDataEntities(query.RootEntity.ObjectDefinitionFullName);
        }    


        

    }
}