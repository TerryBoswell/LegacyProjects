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
        private const string TTL = "20";
        private const string PageSize = "1024";
        private string AccessToken = string.Empty;
        private etouches.Connector createConnector()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add("AccountId", AccountId);
            properties.Add("EventId", EventId);
            properties.Add("ApiKey", ApiKey);
            properties.Add("SubDomain", string.Empty);
            properties.Add("BaseUrl", BaseUrl);
            properties.Add("TTL", TTL);
            properties.Add("PageSize", PageSize);

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
        public void TestListAttendeesWithKeyPairs()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListAttendees(BaseUrl, AccessToken, AccountId, EventId);
            var key = "category_reference";
            Dictionary<string, string> keyPairs = getTestKeyPairs(data, key);
            data = etouches.DataServicesClient.ListAttendees(BaseUrl, AccessToken, AccountId, EventId, null, null, keyPairs);
            var table = data.Tables[0];
            Assert.IsTrue(table.Rows.Count > 0);
            var val = table.Rows[0][key].ToString();
            //have to loop so not to take into account data type
            foreach (var row in table.Rows)
            {
                var curVal = ((DataRow)row)[key].ToString();
                Assert.AreEqual(val, curVal);
            }
        }

        [TestMethod]
        public void TestListEvents()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListEvents(BaseUrl, AccessToken, AccountId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListEventsWithKeyPairs()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListEvents(BaseUrl, AccessToken, AccountId);
            var key = "enddate";
            Dictionary<string, string> keyPairs = getTestKeyPairs(data, key);
            data = etouches.DataServicesClient.ListEvents(BaseUrl, AccessToken, AccountId, null, null, null, keyPairs);
            var table = data.Tables[0];
            Assert.IsTrue(table.Rows.Count > 0);
            var val = table.Rows[0][key].ToString();
            //have to loop so not to take into account data type
            foreach (var row in table.Rows)
            {
                var curVal = ((DataRow)row)[key].ToString();
                Assert.AreEqual(val, curVal);
            }
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
        public void TestListRegSessionsWithKeyPairs()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListRegSessions(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
            var key = "fname";
            Dictionary<string, string> keyPairs = getTestKeyPairs(data, key);
            data = etouches.DataServicesClient.ListRegSessions(BaseUrl, AccessToken, AccountId, EventId, keyPairs);
            var table = data.Tables[0];
            Assert.IsTrue(table.Rows.Count > 0);
            var val = table.Rows[0][key].ToString();
            //have to loop so not to take into account data type
            foreach (var row in table.Rows)
            {
                var curVal = ((DataRow)row)[key].ToString();
                Assert.AreEqual(val, curVal);
            }
        }

        [TestMethod]
        public void TestListSpeakers()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListSpeakers(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListSpeakersWithKeyPairs()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListSpeakers(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
            var key = "speaker_fname";
            Dictionary<string, string> keyPairs = getTestKeyPairs(data, key);
            data = etouches.DataServicesClient.ListSpeakers(BaseUrl, AccessToken, AccountId, EventId, keyPairs);
            var table = data.Tables[0];
            Assert.IsTrue(table.Rows.Count > 0);
            var val = table.Rows[0][key].ToString();
            //have to loop so not to take into account data type
            foreach (var row in table.Rows)
            {
                var curVal = ((DataRow)row)[key].ToString();
                Assert.AreEqual(val, curVal); 
            }
        }

        [TestMethod]
        public void TestListSessions()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListSessions(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListSessionsWithKeyPairs()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListSessions(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
            var key = "sessiondate";
            Dictionary<string, string> keyPairs = getTestKeyPairs(data, key);
            data =  etouches.DataServicesClient.ListSessions(BaseUrl, AccessToken, AccountId, EventId, keyPairs);            
            var table = data.Tables[0];
            Assert.IsTrue(table.Rows.Count > 0);
            var val = table.Rows[0][key].ToString();
            //have to loop so not to take into account data type
            foreach (var row in table.Rows)
            {
                var curVal = ((DataRow)row)[key].ToString();
                Assert.AreEqual(val, curVal);
            }
        }

        [TestMethod]
        public void TestListSessionTracks()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListSessionTracks(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListSessionTracksWithKeyPairs()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListSessionTracks(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
            var key = "eventid";
            Dictionary<string, string> keyPairs = getTestKeyPairs(data, key);
            data = etouches.DataServicesClient.ListSessionTracks(BaseUrl, AccessToken, AccountId, EventId, keyPairs);
            var table = data.Tables[0];
            Assert.IsTrue(table.Rows.Count > 0);
            var val = table.Rows[0][key].ToString();
            //have to loop so not to take into account data type
            foreach (var row in table.Rows)
            {
                var curVal = ((DataRow)row)[key].ToString();
                Assert.AreEqual(val, curVal);
            }
        }


        [TestMethod]
        public void TestListMeetings()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListMeetings(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestListMeetingsWithKeyPairs()
        {
            VerifyAccessToken();
            var data = etouches.DataServicesClient.ListMeetings(BaseUrl, AccessToken, AccountId, EventId);
            Assert.IsNotNull(data);
            var key = "eventid";
            Dictionary<string, string> keyPairs = getTestKeyPairs(data, key);
            data = etouches.DataServicesClient.ListMeetings(BaseUrl, AccessToken, AccountId, EventId, keyPairs);
            var table = data.Tables[0];
            Assert.IsTrue(table.Rows.Count > 0);
            var val = table.Rows[0][key].ToString();
            //have to loop so not to take into account data type
            foreach (var row in table.Rows)
            {
                var curVal = ((DataRow)row)[key].ToString();
                Assert.AreEqual(val, curVal);
            }
        }


        private Dictionary<string, string> getTestKeyPairs(DataSet data, string colName)
        {
            Dictionary<string, string> keyPairs = new Dictionary<string, string>();
            if (data.Tables.Count > 0 && data.Tables[0].Columns.Count > 1 && data.Tables[0].Rows.Count > 1)
            {
                string value = string.Empty;
                for (int i = 0; i < data.Tables[0].Rows.Count; i++)
                {
                    value = data.Tables[0].Rows[i][colName].ToString();
                    if (!String.IsNullOrEmpty(value))
                        break;
                }
                keyPairs.Add(colName, value);
            }
            return keyPairs;
        }
        #endregion
    }
}
