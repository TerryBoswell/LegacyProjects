using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Xml;
namespace Scribe.Connector.Test
{
    [TestClass]
    public class DataServicesClient_Test
    {
        private const string AccountId = "1";
        private const string EventId = "34762";
        private const string ApiKey = "ae9d6a1ff39db2e78eccd40bc2f6621bd052f990";
        private const string BaseUrl = "https://stage-ds.etouches.com";
        private string AccessToken = string.Empty;
        private etouches.Connector createConnector()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add("AccountId", AccountId);
            properties.Add("EventId", EventId);
            properties.Add("ApiKey", ApiKey);
            properties.Add("SubDomain", string.Empty);
            properties.Add("BaseUrl", BaseUrl);

            var connector = new Scribe.Connector.etouches.Connector();

            connector.Connect(properties);
            return connector;
        }

        private void VerifyAccessToken()
        {
            if (String.IsNullOrEmpty(AccessToken))
                AccessToken = etouches.DataServicesClient.Authorize(BaseUrl, AccountId, ApiKey);
        }

        [TestMethod]
        public void TestCanConnect()
        {
            var connector = createConnector();
            Assert.IsNotNull(connector);
            Assert.AreEqual(connector.IsConnected, true);            
        }

        #region Meta Data Tests
        [TestMethod]
        public void TestGetAttendeeMetaData()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.GetAttendeeMetaData(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestGetRegSessionMetaData()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.GetRegSessionMetaData(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestGetSessionMetaData()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.GetSessionMetaData(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestGetSpeakerMetaData()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.GetSpeakerMetaData(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestGetSessionTrackMetaData()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.GetSessionTrackMetaData(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestGetMeetingMetaData()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.GetMeetingMetaData(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        #endregion

        #region List Tests
        [TestMethod]
        public void TestListAttendees()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListAttendees(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListEvents()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListEvents(BaseUrl, AccessToken, AccountId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListEventsWithLessThanDate()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListEvents(BaseUrl, AccessToken, AccountId, null, DateTime.Now.AddDays(-30));
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListEventsWithAttendeeDate()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListEvents(BaseUrl, AccessToken, AccountId, null, null, DateTime.Now.AddDays(-30));
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListRegSessions()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListRegSessions(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListSpeakers()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListSpeakers(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListSessions()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListSessions(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListSessionTracks()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListSessionTracks(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListMeetings()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListMeetings(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }


        #endregion
    }
}
