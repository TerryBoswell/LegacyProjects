using Scribe.Core.ConnectorApi.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private string subDomain = string.Empty;
        private string uri = string.Empty;
        
        public ScribeConnection(IDictionary<string, string> properties)
        {
            AccountId = properties["AccountId"];
            Int32 numericResult = 0;
            Int32.TryParse(AccountId, out numericResult);
            if (numericResult == 0) throw new ApplicationException("Account Id must be numeric.");

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
        }

        private List<ConnectionHistory> history;
        public List<ConnectionHistory> History
        {
            get { return history; }
        }

        public bool TryConnect()
        {
            AccessToken = DataServicesClient.Authorize(BaseUrl, AccountId, this.apiKey);
            history.Add(new ConnectionHistory(AccessToken) { });
            if (String.IsNullOrEmpty(AccessToken))
                Logger.Write(Logger.Severity.Error, ObjectDefinitions.Constants.ConnectorTitle, "Connection Failed");
            else
                Logger.Write(Logger.Severity.Info, ObjectDefinitions.Constants.ConnectorTitle, "Connection Established");
            return true;
        }

        public void ReConnnect()
        {
            Logger.Write(Logger.Severity.Info, "ScribeConnection", "Reconnecting Session");
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
