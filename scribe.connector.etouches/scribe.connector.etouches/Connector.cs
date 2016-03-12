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

        public const string API_URI_PATTERN = @"https://{0}.eiseverywhere.com";

        public static string EventId = String.Empty;
        public static string AccountId = String.Empty;
        public static string TTL = "20";
        public static int PageSize = 1024;
        private string apiKey = String.Empty;
        private string subDomain = string.Empty;
        private string uri = string.Empty;
        public static string BaseUrl = "https://eiseverywhere.com";
        public static string AccessToken = string.Empty;

        public void Connect(IDictionary<string, string> properties)
        {
            //retrieve and test the AccountId
            AccountId = properties["AccountId"];
            Int32 numericResult = 0;
            Int32.TryParse(AccountId, out numericResult);
            if (numericResult == 0) throw new ApplicationException("Account Id must be numeric.");

            //retrieve and test the EventId
            EventId = properties["EventId"];
            numericResult = 0;
            Int32.TryParse(EventId, out numericResult);
            if (numericResult == 0) throw new ApplicationException("Event Id must be numeric.");

            //retrieve and test the EventId
            if (properties.ContainsKey("TTL"))
            {
                TTL = properties["TTL"];
                numericResult = 0;
                Int32.TryParse(TTL, out numericResult);
                if (numericResult == 0) throw new ApplicationException("TTL must be numeric.");
            }
            else
                TTL = "20";

            //retrieve the page size
            if (properties.ContainsKey("PageSize"))
            {
                var pageSize = properties["PageSize"];
                var intResult = 0;
                if (Int32.TryParse(pageSize, out intResult))
                    PageSize = intResult;
            }

            //retrieve the ApiKey, scribe's UI ensures its not empty
            this.apiKey = properties["ApiKey"];

            //retrieve the SubDomain, this can be empty
            this.subDomain = properties["SubDomain"];
            if (string.IsNullOrEmpty(subDomain)) subDomain = "www";
            //remove any trailing "."
            if (subDomain.EndsWith(".")) subDomain = subDomain.Remove(subDomain.Length - 1);


            if (!String.IsNullOrEmpty(properties["BaseUrl"]))
            {
                BaseUrl = properties["BaseUrl"];
            }
            else
            {
                //use the default eiseverywhere.com
                var uri = new UriBuilder(BaseUrl);
                BaseUrl = uri.ToString();
                //set the api URI context we'll be using for this connection (qa, supportqa, etc...)
                BaseUrl = String.Format(API_URI_PATTERN, this.subDomain);
            }

            //attempt connection & set connection status
            this.IsConnected = TryConnect();
        }

        private bool TryConnect()
        {
            AccessToken = DataServicesClient.Authorize(BaseUrl, AccountId, this.apiKey);
            return true;
        }

        //private bool TryConnect2()
        //{

        //    var http = new EzHttp.HttpClient(this.baseUrl);
        //    http.LoggingEnabled = true;
        //    http.StreamResponse = false;
        //    var uri = new UriBuilder(this.baseUrl);
        //    var result = http.Get("/api/v2/datasvc/authorize.json", new { accountid = this.accountId, key = this.apiKey });

        //    var resp = result.DynamicBody;
        //    Debug.Write(resp.response.etouches.accesstoken);
        //    return false;
            
        //    //var resp = XDocument.Load(result.ResponseStream);
        //    //var el = resp.XPathSelectElements("/response/etouches/accesstoken").FirstOrDefault();
        //    //if (el != null)
        //    //{
        //    //    this.accessToken = el.Value;
        //    //    return true;
        //    //} 
        //    //else
        //    //{
        //    //    throw new ApplicationException("Authentication Failed.\n\n" + resp.Document.ToString());
        //    //}
        //}


        //private bool TryConnect()
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(this.baseUrl);
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        //SERVER/api/v2/datasvc/authorize.xml?accountid=1&key=2
        //        var uri = new UriBuilder(this.baseUrl);
        //        var qs = HttpUtility.ParseQueryString(string.Empty);
        //        qs["acountid"] = this.accountId;
        //        qs["key"] = this.apiKey;
        //        uri.Query = qs.ToString();
        //        uri.Path = "/api/v2/datasvc/authorize.xml";

        //        HttpResponseMessage response = client.GetAsync(uri.ToString()).Result; //await client.GetAsync(uri.ToString());
        //        if (response.IsSuccessStatusCode)
        //        {
        //            return true;
        //            Product product = await response.
        //            Console.WriteLine("{0}\t${1}\t{2}", product.Name, product.Price, product.Category);
        //        }
        //    }
        //    return false;
        //}

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
                    var evnt = new ObjectDefinitions.Event(Connector.AccountId, Connector.EventId);
                    return evnt.ExecuteQuery(query);
                case "Attendee":
                    var attendee = new ObjectDefinitions.Attendee(Connector.AccountId, Connector.EventId);
                    return attendee.ExecuteQuery(query);
                case "RegSession":
                    var regSession = new ObjectDefinitions.RegSession(Connector.AccountId, Connector.EventId);
                    return regSession.ExecuteQuery(query);
                case "Session":
                    var session = new ObjectDefinitions.Session(Connector.AccountId, Connector.EventId);
                    return session.ExecuteQuery(query);
                case "Meeting":
                    var meeting = new ObjectDefinitions.Meeting(Connector.AccountId, Connector.EventId);
                    return meeting.ExecuteQuery(query);
                case "Speaker":
                    var speaker = new ObjectDefinitions.Speaker(Connector.AccountId, Connector.EventId);
                    return speaker.ExecuteQuery(query);
                case "SessionTrack":
                    var sessiontrack = new ObjectDefinitions.SessionTrack(Connector.AccountId, Connector.EventId);
                    return sessiontrack.ExecuteQuery(query);
                default:
                    throw new NotImplementedException();
            }
            
        }


        private IMetadataProvider metadataProvider;

        public IMetadataProvider GetMetadataProvider()
        {
            this.metadataProvider = new MetadataProvider(AccountId, EventId);
            return this.metadataProvider;            
        }

        public bool IsConnected { get; set; }

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
                        },
                        new EntryDefinition
                        {
                            InputType = InputType.Text,                               
                            IsRequired = false,
                            Label = "Sub Domain (qa, supportqa, etc...)",
                            PropertyName = "SubDomain"
                        },
                        new EntryDefinition
                        {
                            InputType = InputType.Text,                               
                            IsRequired = false,
                            Label = "Base URL (leave blank for https://eiseverywhere.com)",
                            PropertyName = "BaseUrl"
                        }
                    } 


                };

            return form.Serialize();
        }
    }
}
