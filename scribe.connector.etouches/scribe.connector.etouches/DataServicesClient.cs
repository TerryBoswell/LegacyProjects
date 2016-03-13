using EasyHttp.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Scribe.Connector.etouches
{
    
    public static class DataServicesClient
    {
        #region Reserved Property Names For Rest Calls
        /// <summary>
        /// This is the property we will look for to pass to the lastmodified-lt query parameter in selected values
        /// </summary>
        public static string LastModifiedParameter = "lastmodified";

        public static string AttendeeLastModifiedParameter = "attendees_lastmodified";
        #endregion

        public static string Authorize(string baseUrl, string accountId, string apiKey)
        {
            var http = NewHttpClient();
            var uri = new UriBuilder(baseUrl);
            uri.Path = "authenticate"; 

            var result = http.Get(uri.ToString(), new { accountid = accountId, key = apiKey });
            var json = JObject.Parse(result.RawText);
            if(((string)json["status"]).ToLower()=="error")
                throw new ApplicationException((string)json["msg"]);
            if (!String.IsNullOrEmpty((string)json["accesstoken"])) 
                return json["accesstoken"].ToString();
            throw new ApplicationException(result.RawText);
        }

        #region Meta Data
        ///eventmetadata/
        public static JObject GetEventMetaData(string baseUrl, string accesstoken, string accountId)
        {
            return GetJObject(baseUrl, "eventmetadata.json", accesstoken, accountId);            
        }

        ///sessionmetadata/
        public static JObject GetSessionMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetJObject(baseUrl, "sessionmetadata.json", accesstoken, accountId, eventId);            
        }

        ///attendeemetadata/
        public static JObject GetAttendeeMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetJObject(baseUrl, "attendeemetadata.json", accesstoken, accountId, eventId);            
        }
        ///regsessionmetadata/        
        public static JObject GetRegSessionMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetJObject(baseUrl, "regsessionmetadata.json", accesstoken, accountId, eventId);            
        }

        ///speakermetadata/
        public static JObject GetSpeakerMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetJObject(baseUrl, "speakermetadata.json", accesstoken, accountId, eventId);
        }

        /*
        GET /sessiontrackmetadata/[accountid]/[eventid] accountid
        */
        public static JObject GetSessionTrackMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetJObject(baseUrl, "sessiontrackmetadata.json", accesstoken, accountId, eventId);
        }


        /*
        GET /meetingmetadata/[accountid]/[eventid] accountid
        */
        public static JObject GetMeetingMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetJObject(baseUrl, "meetingmetadata.json", accesstoken, accountId, eventId);
        }
        #endregion 

        #region List Data
        /*
        /eventlist/[accountid]*
        deleted (optional)
        lastmodified-gt
        (optional)
        lastmodified-lt (optional)
        attendees_modified-gt
        (optional)
        attendees_modified-lt
        (optional)
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListEvents(string baseUrl, string accesstoken, string accountId, 
            DateTime? modifiedAfter = null, DateTime? modifiedBefore = null, DateTime? attendeesModifiedAfter = null,
            Dictionary<string, string> keypairs = null)
        {
            string aQuery = string.Empty;
            if (attendeesModifiedAfter.HasValue)
            {
                var d = attendeesModifiedAfter.Value.ToString("yyyy-MM-dd");
                aQuery = $"attendees_modified-gt={d}";
            }
            var action = "eventlist.json";
            var key = Generatekey(action, accountId, null, aQuery, keypairs);
            DataSet ds = ConnectorCache.GetCachedData<DataSet>(key);
            if (ds != null)
                return ds;
            ds = GetDatasetIteratively(baseUrl, action, accesstoken, accountId, null, modifiedAfter, modifiedBefore, 
                aQuery, keypairs);
            if (ds != null)
                ConnectorCache.StoreData(key, ds);
            return ds;
        }

        /*
        /attendeelist/[accountid]/[eventid]*
        deleted (optional)
        lastmodified-gt
        (optional)
        lastmodified-lt (optional)
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListAttendees(string baseUrl, string accesstoken, string accountId, string eventId, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null,
            Dictionary<string, string> keypairs = null)
        {
            var action = "attendeelist.json";
            var key = Generatekey(action, accountId, eventId, null, keypairs);
            DataSet ds = ConnectorCache.GetCachedData<DataSet>(key);
            if (ds != null)
                return ds;
            ds = GetDatasetIteratively(baseUrl, action, accesstoken, accountId, eventId, modifiedAfter, modifiedBefore, null, keypairs);
            if (ds != null)
                ConnectorCache.StoreData(key, ds);
            return ds;
        }
        /*
        /regsessionlist/[accountid]/[eventid]*
        deleted (optional)
        lastmodified-gt
        (optional)
        lastmodified-lt (optional)
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListRegSessions(string baseUrl, string accesstoken, string accountId, string eventId,
            Dictionary<string, string> keypairs = null)
        {
            var action = "regsessionlist.json";
            var key = Generatekey(action, accountId, eventId, null, keypairs);
            DataSet ds = ConnectorCache.GetCachedData<DataSet>(key);
            if (ds != null)
                return ds;
            ds = GetDatasetIteratively(baseUrl, action, accesstoken, accountId, eventId, keypairs);
            if (ds != null)
                ConnectorCache.StoreData(key, ds);
            return ds;
        }

        /*
        /speakerlist/[accountid]/[eventid] *
        deleted (optional)
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListSpeakers(string baseUrl, string accesstoken, string accountId, string eventId,
            Dictionary<string, string> keypairs = null)
        {
            var action = "speakerlist.json";
            var key = Generatekey(action, accountId, eventId, null, keypairs);
            DataSet ds = ConnectorCache.GetCachedData<DataSet>(key);
            if (ds != null)
                return ds;
            ds = GetDatasetIteratively(baseUrl, action, accesstoken, accountId, eventId, keypairs);
            if (ds != null)
                ConnectorCache.StoreData(key, ds);
            return ds;
        }


        /*
        /sessionlist/[accountid]/[eventid] *
        deleted (optional)
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListSessions(string baseUrl, string accesstoken, string accountId, string eventId,
            Dictionary<string, string> keypairs = null)
        {
            var action = "sessionlist.json";
            var key = Generatekey(action, accountId, eventId, null, keypairs);
            DataSet ds = ConnectorCache.GetCachedData<DataSet>(key);
            if (ds != null)
                return ds;
            ds = GetDatasetIteratively(baseUrl, action, accesstoken, accountId, eventId, keypairs);
            if (ds != null)
                ConnectorCache.StoreData(key, ds);
            return ds;
        }

        /*
        /sessiontracklist/[accountid]/[eventid] *
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListSessionTracks(string baseUrl, string accesstoken, string accountId, string eventId,
            Dictionary<string, string> keypairs = null)
        {
            var action = "sessiontracklist.json";
            var key = Generatekey(action, accountId, eventId, null, keypairs);
            DataSet ds = ConnectorCache.GetCachedData<DataSet>(key);
            if (ds != null)
                return ds;
            ds = GetDatasetIteratively(baseUrl, action, accesstoken, accountId, eventId, keypairs);
            if (ds != null)
                ConnectorCache.StoreData(key, ds);
            return ds;
        }

        /*
        /meetinglist/[accountid]/[eventid] *
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListMeetings(string baseUrl, string accesstoken, string accountId, string eventId,
            Dictionary<string, string> keypairs = null)
        {
            var action = "meetinglist.json";
            var key = Generatekey(action, accountId, eventId, null, keypairs);
            DataSet ds = ConnectorCache.GetCachedData<DataSet>(key);
            if (ds != null)
                return ds;
            ds = GetDatasetIteratively(baseUrl, action, accesstoken, accountId, eventId, keypairs);
            if (ds != null)
                ConnectorCache.StoreData(key, ds);
            return ds;
        }

        #endregion




        #region Utility Methods

        private static string Generatekey(string action, string accountId, string eventId = null, 
            string aQuery = null, Dictionary<string, string> keypairs = null)
        {
            var key = $"{Connector.AccessToken}-{action}-{accountId}-{eventId}-{aQuery}";
            if (keypairs != null && keypairs.Any())
                foreach (var kp in keypairs)
                    key = $"{key}-{kp.Key}-{kp.Value}";
            return key;
        }
        public static HttpClient NewHttpClient(string baseUri = null)
        {
            var http = new HttpClient(baseUri);
            http.LoggingEnabled = true;
            http.StreamResponse = false;
            return http;
        }

        /// <summary>
        /// This method will load the data set using the Json Serialize directly to a dataset
        /// This method will fail on the loading of entities with properties that are collections
        /// This method does not convert unix date representation of null dates
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="action"></param>
        /// <param name="accesstoken"></param>
        /// <param name="accountId"></param>
        /// <param name="eventId"></param>
        /// <param name="modifiedAfter"></param>
        /// <param name="modifiedBefore"></param>
        /// <returns></returns>
        private static DataSet GetDataset(string baseUrl, string action, string accesstoken, string accountId = null,
            string eventId = null, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null, string additionCondition = null)
        {
            HttpResponse result = DoHttpGetInternal(baseUrl, action, accesstoken, accountId, eventId, modifiedAfter, modifiedBefore, additionCondition);
            if (result == null)
                throw new ApplicationException("Result of get was null");
            DataSet ds = null;
            string plainJson = result.RawText;
            try
            {
                ds = JsonConvert.DeserializeObject<DataSet>(plainJson);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error : {ex.Message} while deserializing {plainJson}");
            }
            return ds;
        }

        private static JObject GetJObject(string baseUrl, string action, string accesstoken, string accountId = null,
            string eventId = null)
        {
            HttpResponse result = DoHttpGetInternal(baseUrl, action,  accesstoken, accountId, eventId);
            if (result == null)
                throw new ApplicationException("Result of get was null");
            var res = result.RawText;
            var json = JObject.Parse(res);
            return json;
        }


        private static DataSet GetDatasetIteratively(string baseUrl, string action, string accesstoken, string accountId = null,
            string eventId = null, Dictionary<string, string> keypairs = null)
        {
            return GetDatasetIteratively(baseUrl, action, accesstoken, accountId, eventId, null, null ,null, keypairs);
        }
        /// <summary>
        /// This method will load a dataset iteratively. It converts one row and column at a time
        /// it protects against properties that are collections ignoring them
        /// it will protect against unix representation of null dates
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="action"></param>
        /// <param name="accesstoken"></param>
        /// <param name="accountId"></param>
        /// <param name="eventId"></param>
        /// <param name="modifiedAfter"></param>
        /// <param name="modifiedBefore"></param>
        /// <returns></returns>
        private static DataSet GetDatasetIteratively(string baseUrl, string action, string accesstoken, string accountId = null,
            string eventId = null, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null, string additionCondition = null,
            Dictionary<string, string> keypairs = null)
        {
            HttpResponse result = DoHttpGetInternal(baseUrl, action, accesstoken,
                accountId, eventId, modifiedAfter, modifiedBefore, additionCondition, Connector.PageSize, keypairs);
            if (result == null)
                throw new ApplicationException("Result of get was null");
            DataSet ds = null;
            string plainJson = result.RawText;
            try
            {
                ds = ConvertDataSetIteratively(plainJson);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error : {ex.Message} while deserializing {plainJson}");
            }
            return ds;
        }
        public static DataSet ConvertDataSetIteratively(string json)
        {
            var jsonLinq = JObject.Parse(json);

            // Find the first array using Linq
            var srcArray = jsonLinq.Descendants().Where(d => d is JArray).First();
            var trgArray = new JArray();
            foreach (JObject row in srcArray.Children<JObject>())
            {
                var cleanRow = new JObject();
                foreach (JProperty column in row.Properties())
                {
                    // Only include JValue types
                    if (column.Value is JValue)
                    {
                        //We need to clean up columns of type date that are represented this way for nulls
                        if (column.Value.ToString().Equals("0000-00-00 00:00:00", StringComparison.OrdinalIgnoreCase))
                            column.Value = System.DateTime.MinValue.ToString("yyyy-MM-dd");
                        if (column.Value.ToString().Equals("0000 - 00 - 00", StringComparison.OrdinalIgnoreCase))
                            column.Value = System.DateTime.MinValue.ToString("yyyy-MM-dd");
                        if (column.Value.ToString().Equals("0000-00-00", StringComparison.OrdinalIgnoreCase))
                            column.Value = System.DateTime.MinValue.ToString("yyyy-MM-dd");
                        if (column.Value.ToString().Equals("0001-01-01 00:00:00", StringComparison.OrdinalIgnoreCase))
                            column.Value = System.DateTime.MinValue.ToString("yyyy-MM-dd");
                        if (column.Value.ToString().Equals("0001 - 01 - 01", StringComparison.OrdinalIgnoreCase))
                            column.Value = System.DateTime.MinValue.ToString("yyyy-MM-dd");
                        if (column.Value.ToString().Equals("0001-01-01", StringComparison.OrdinalIgnoreCase))
                            column.Value = System.DateTime.MinValue.ToString("yyyy-MM-dd");
                        cleanRow.Add(column.Name, column.Value);
                    }
                }

                trgArray.Add(cleanRow);
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(JsonConvert.DeserializeObject<DataTable>(trgArray.ToString()));
            ds.Tables[0].TableName = "ResultSet";
            return ds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="action"></param>
        /// <param name="accesstoken"></param>
        /// <param name="accountId"></param>
        /// <param name="eventId"></param>
        /// <param name="modifiedAfter">Using standard lastmodified-gt parameter</param>
        /// <param name="modifiedBefore">Using standard lastmodified-lt=</param>
        /// <param name="additionCondition">A custom Query parameter to pass that is not a common one</param>
        /// <returns></returns>
        private static HttpResponse DoHttpGetInternal(string baseUrl, string action, string accesstoken, string accountId = null,
            string eventId = null, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null, string additionCondition = null,
            int? pageSize = null, Dictionary<string, string> keypairs = null)
        {
            var http = NewHttpClient();
            //var uri = new UriBuilder(baseUrl);
            if (String.IsNullOrEmpty(action))
                throw new ApplicationException("An Action must be provided");
            if (String.IsNullOrEmpty(baseUrl))
                throw new ApplicationException("A base url must be provided");
            if (String.IsNullOrEmpty(accesstoken))
                throw new ApplicationException("An access token must be provided");
            if (String.IsNullOrEmpty(accountId) && !String.IsNullOrEmpty(eventId))
                throw new ApplicationException("An account id must be provided when an event id is provided");

            var path = string.Empty;
            if (String.IsNullOrEmpty(eventId))
                path = $"{baseUrl}/{action}/{accountId}?accesstoken={accesstoken}";
            else
                path = $"{baseUrl}/{action}/{accountId}/{eventId}?accesstoken={accesstoken}";

            if (modifiedBefore.HasValue)
            {
                var ltDate = modifiedBefore.Value.ToString("yyyy-MM-dd");
                path = $"{path}&lastmodified-lt='{ltDate}'";
            }
            if (modifiedAfter.HasValue)
            {
                var gtDate = modifiedAfter.Value.ToString("yyyy-MM-dd");
                path = $"{path}&lastmodified-gt='{gtDate}'";
            }

            if (!String.IsNullOrEmpty(additionCondition))
                path = $"{path}&{additionCondition}";

            if (pageSize.HasValue)
                path = $"{path}&pageSize={pageSize.Value}";

            if (keypairs != null && keypairs.Any())
                foreach (var kp in keypairs)
                    path += $"&{kp.Key}={kp.Value}";
            
            return http.Get(path);
        }

       

        #endregion
        

    }

}
