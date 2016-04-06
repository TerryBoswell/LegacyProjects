using Scribe.Connector.etouches.ObjectDefinitions;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.ConnectionUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Scribe.Connector.etouches
{
    [ScribeConnector(
        ConnectorSettings.ConnectorTypeId,
        ConnectorSettings.Name,
        ConnectorSettings.Description,
        typeof(Connector),
        StandardConnectorSettings.SettingsUITypeName,
        StandardConnectorSettings.SettingsUIVersion,
        StandardConnectorSettings.ConnectionUITypeName,
        StandardConnectorSettings.ConnectionUIVersion,
        StandardConnectorSettings.XapFileName,
        new[] { "Scribe.IS.Source", "Scribe.IS.Target", "Scribe.IS2.Source", "Scribe.IS2.Target" },
        ConnectorSettings.SupportsCloud, ConnectorSettings.ConnectorVersion
        )]
    public class Connector : IConnector
    {

        public ScribeConnection Connection;
        public ScribeConnection V2Connection;

        public void Connect(IDictionary<string, string> properties)
        {
            this.Connection = new ScribeConnection(properties, ScribeConnection.ConnectionVersion.V1);
            //attempt connection & set connection status
            this.IsConnected = this.Connection.TryConnect();

            try
            {
                this.V2Connection = new ScribeConnection(properties, ScribeConnection.ConnectionVersion.V2);
                this.IsV2Connected = this.V2Connection.TryConnect();
            }
            catch 
            { }
        }

        
        public void SetPageSize(int pageSize)
        {
            Connection.PageSize = pageSize;
        }
        public Guid ConnectorTypeId
        {
            get { return new Guid(ConnectorSettings.ConnectorTypeId); }
        }

        public void Disconnect()
        {
            this.IsConnected = false;
        }

        public MethodResult ExecuteMethod(MethodInput input)
        {
            throw new NotImplementedException();
        }

        public OperationResult ExecuteOperation(OperationInput input)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DataEntity> ExecuteQuery(Core.ConnectorApi.Query.Query query)
        {           
            switch(query.RootEntity.ObjectDefinitionFullName)
            {
                case "Event":
                    var evnt = new ObjectDefinitions.Event(this.Connection);
                    return evnt.ExecuteQuery(query);
                case "Attendee":
                    var attendee = new ObjectDefinitions.Attendee(this.Connection);
                    return attendee.ExecuteQuery(query);
                case "RegSession":
                    var regSession = new ObjectDefinitions.RegSession(this.Connection);
                    return regSession.ExecuteQuery(query);
                case "Session":
                    var session = new ObjectDefinitions.Session(this.Connection);
                    return session.ExecuteQuery(query);
                case "Meeting":
                    var meeting = new ObjectDefinitions.Meeting(this.Connection);
                    return meeting.ExecuteQuery(query);
                case "Speaker":
                    var speaker = new ObjectDefinitions.Speaker(this.Connection);
                    return speaker.ExecuteQuery(query);
                case "SessionTrack":
                    var sessiontrack = new ObjectDefinitions.SessionTrack(this.Connection);
                    return sessiontrack.ExecuteQuery(query);
                case "FinacialTransaction":
                    var financialTransaction = new ObjectDefinitions.FinancialTransaction(this.Connection);
                    return financialTransaction.ExecuteQuery(query);
                    
                default:
                    throw new NotImplementedException();
            }
            
        }


        private IMetadataProvider metadataProvider;

        public IMetadataProvider GetMetadataProvider()
        {
            this.metadataProvider = new MetadataProvider(this.Connection);
            return this.metadataProvider;            
        }

        public bool IsConnected { get; set; }

        public bool IsV2Connected { get; set; }
        public string PreConnect(IDictionary<string, string> properties)
        {
            var form = new FormDefinition
                {
                    CompanyName = "etouches, Inc.",
                    CryptoKey = "{B5D0EEE1-40CE-4D34-B161-52B8620903EE}",
                    HelpUri = new Uri("https://developer.etouches.com/"),
                    Entries = new Collection<EntryDefinition>
                    {
                        new EntryDefinition
                        {
                            InputType = InputType.Text,
                            IsRequired = true,
                            Label = "Account Id",
                            PropertyName = "AccountId"
                        },
                        new EntryDefinition
                        {
                            InputType = InputType.Text,
                            IsRequired = true,
                            Label = "Event Id",
                            PropertyName = "EventId"
                        },
                        new EntryDefinition
                        {
                            InputType = InputType.Text,                               
                            IsRequired = true,
                            Label = "API Key",
                            PropertyName = "ApiKey"
                        },
                        new EntryDefinition
                        {
                            InputType = InputType.Text,
                            IsRequired = true,
                            Label = "TTL for Cache In Minutes",
                            PropertyName = "TTL"
                        },
                        new EntryDefinition
                        {
                            InputType = InputType.Text,
                            IsRequired = true,
                            Label = "Page Size",
                            PropertyName = "PageSize"
                        }                    } 


                };

            return form.Serialize();
        }
    }
}
