using Scribe.Core.ConnectorApi.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Connector.etouches
{
    public class ScribeConnection
    {
        public const string API_URI_PATTERN = @"https://{0}.eiseverywhere.com";

        internal string AccessToken = string.Empty;
        internal string EventId = String.Empty;
        internal string AccountId = String.Empty;
        internal string BaseUrl = "https://eiseverywhere.com";
        internal bool IsConnected = false;
        internal int TTL = 20;
        internal int PageSize = 1024;

        private string apiKey = String.Empty;
        //private string subDomain = string.Empty;
        private string uri = string.Empty;

        public enum ConnectionVersion
        {
            V1,
            V2
        }

        internal ConnectionVersion Version;

        public ScribeConnection(IDictionary<string, string> properties, ConnectionVersion version)
        {
            this.Version = version;

            connectionKey = Guid.NewGuid();
            var keyName =  "AccountId";
            
            Int32 numericResult = 0;
            if (properties.ContainsKey(keyName))
            { 
                AccountId = properties[keyName];
                Int32.TryParse(AccountId, out numericResult);
                if (numericResult == 0) throw new ApplicationException("Account Id must be numeric.");
            }
            //retrieve and test the EventId
            EventId = properties["EventId"];
            numericResult = 0;
            Int32.TryParse(EventId, out numericResult);
            if (numericResult == 0) throw new ApplicationException("Event Id must be numeric.");

           
            //retrieve the page size
            if (properties.ContainsKey("TTL"))
            {
                var ttl = properties["TTL"];
                var intResult = 0;
                if (Int32.TryParse(ttl, out intResult))
                    TTL = intResult;
                else
                    TTL = 20;
            }

            //retrieve the page size
            if (properties.ContainsKey("PageSize"))
            {
                var pageSize = properties["PageSize"];
                var intResult = 0;
                if (Int32.TryParse(pageSize, out intResult))
                    PageSize = intResult;
                else
                    PageSize = 1024;
            }

            //retrieve the ApiKey, scribe's UI ensures its not empty
            if (properties.ContainsKey("ApiKey"))
                this.apiKey = properties["ApiKey"];
            
            //retrieve the SubDomain, this can be empty
            //this.subDomain = properties["SubDomain"];
            //if (string.IsNullOrEmpty(subDomain)) subDomain = "www";
            //remove any trailing "."
            //if (subDomain.EndsWith(".")) subDomain = subDomain.Remove(subDomain.Length - 1);

            if (version == ConnectionVersion.V1)
            {
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
                    //BaseUrl = String.Format(API_URI_PATTERN, this.subDomain);
                }
            }
            else if (version == ConnectionVersion.V2)
            {
                if (properties.ContainsKey("V2Url") && !String.IsNullOrEmpty(properties["V2Url"]))
                {
                    BaseUrl = properties["V2Url"];
                }
                else
                {
                    //use the default eiseverywhere.com
                    var uri = new UriBuilder(BaseUrl);
                    BaseUrl = uri.ToString();
                    //set the api URI context we'll be using for this connection (qa, supportqa, etc...)
                    BaseUrl = String.Format(API_URI_PATTERN, string.Empty);
                }
            }
           
            history = new List<ConnectionHistory>();
        }

        private List<ConnectionHistory> history;
        public List<ConnectionHistory> History
        {
            get { return history; }
        }

        private readonly Guid connectionKey;
        /// <summary>
        /// This represents a unique way of identifying each connection
        /// We cannot use access token because that may change if we reauthenticate
        /// </summary>
        public Guid ConnectionKey
        {
            get { return connectionKey; }
        }

        public bool TryConnect()
        {
            if (Version == ConnectionVersion.V1)
                AccessToken = DataServicesClient.Authorize(BaseUrl, AccountId, this.apiKey);
            else
                AccessToken = ApiV2Client.Authorize(BaseUrl, AccountId, this.apiKey);
            history.Add(new ConnectionHistory(AccessToken) { });
            if (String.IsNullOrEmpty(AccessToken))
                Logger.WriteError("Connection Failed");
            else
                Logger.WriteInfo("Connection Established");
            return true;
        }

        public void ReConnnect()
        {
            Logger.WriteInfo("Reconnecting Session");
            IsConnected = TryConnect();
        }
    }

    public class ConnectionHistory
    {
        private readonly DateTime startTime;
        private readonly string accessToken;
        public DateTime StartTime
        {
            get { return startTime; }
        }
        public string AccessToken
        {
            get { return accessToken; }
        }
        public ConnectionHistory(string accessToken)
        {
            this.startTime = DateTime.Now;
            this.accessToken = accessToken;
        }
    }
}
