using EasyHttp.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Serilog;

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
            Log.Debug("Authenticate: {request}", http.Request.Uri);
            var json = JObject.Parse(result.RawText);
            if(((string)json["status"]).ToLower()=="error")
                throw new ApplicationException((string)json["msg"]);
            if (!String.IsNullOrEmpty((string)json["accesstoken"])) 
                return json["accesstoken"].ToString();
            throw new ApplicationException(result.RawText);
        }

        #region Meta Data

        //utility method to cache meatadata calls
        private static JObject getMetaData(string baseUrl, string action, string accesstoken, string accountId, string eventId)
        {
            var key = Generatekey(action, accountId, eventId, null);
            JObject o = ConnectorCache.GetCachedData<JObject>(key);
            if (o != null)
                return o;
            o = GetJObject(baseUrl, action, accesstoken, accountId, eventId);
            if (o != null)
                ConnectorCache.StoreData(key, o);
            return o;
        }

        ///eventmetadata/
        public static JObject GetEventMetaData(string baseUrl, string accesstoken, string accountId)
        {
            return getMetaData(baseUrl, "eventmetadata.json", accesstoken, accountId, string.Empty);
            //return GetJObject(baseUrl, "eventmetadata.json", accesstoken, accountId);            
        }

        ///sessionmetadata/
        public static JObject GetSessionMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return getMetaData(baseUrl, "sessionmetadata.json", accesstoken, accountId, eventId);
            //return GetJObject(baseUrl, "sessionmetadata.json", accesstoken, accountId, eventId);            
        }

        ///attendeemetadata/
        public static JObject GetAttendeeMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return getMetaData(baseUrl, "attendeemetadata.json", accesstoken, accountId, eventId);
            // return GetJObject(baseUrl, "attendeemetadata.json", accesstoken, accountId, eventId);            
        }

        ///regsessionmetadata/        
        public static JObject GetRegSessionMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return getMetaData(baseUrl, "regsessionmetadata.json", accesstoken, accountId, eventId);
            //return GetJObject(baseUrl, "regsessionmetadata.json", accesstoken, accountId, eventId);            
        }

        ///speakermetadata/
        public static JObject GetSpeakerMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return getMetaData(baseUrl, "speakermetadata.json", accesstoken, accountId, eventId);
            //return GetJObject(baseUrl, "speakermetadata.json", accesstoken, accountId, eventId);
        }

        /*
        GET /sessiontrackmetadata/[accountid]/[eventid] accountid
        */
        public static JObject GetSessionTrackMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return getMetaData(baseUrl, "sessiontrackmetadata.json", accesstoken, accountId, eventId);
            //return GetJObject(baseUrl, "sessiontrackmetadata.json", accesstoken, accountId, eventId);
        }


        /*
        GET /meetingmetadata/[accountid]/[eventid] accountid
        */
        public static JObject GetMeetingMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return getMetaData(baseUrl, "meetingmetadata.json", accesstoken, accountId, eventId);
            //return GetJObject(baseUrl, "meetingmetadata.json", accesstoken, accountId, eventId);
        }
        #endregion 

        #region List Data

        //get a list of objects checking cache first
        private static DataSet getList(string baseUrl, string accesstoken, string accountId, string eventId, Dictionary<string, string> keypairs, string action)
        {

            //check for a cache hit using request as key
            var key = Generatekey(action, accountId, eventId, null, keypairs);
            DataSet ds = ConnectorCache.GetCachedData<DataSet>(key);
            if (ds != null)
            {
                return ds;
            } else
            {
                ds = new DataSet();
            }

            //setup paging
            int pageNum = 1; bool shouldPage = true;
            if (keypairs!=null && keypairs.Keys.Contains("pageNumber"))
            {
                shouldPage = false;
            } else
            {
                if (keypairs == null) keypairs = new Dictionary<string, string>();
                keypairs.Add("pageNumber", pageNum.ToString());
            }
        
            //get first requested page
            var dsCurrentPage = GetDatasetIteratively(baseUrl, action, accesstoken, accountId, eventId, keypairs);

            //only page the remaining if a pageNumber wasnt already specified by the caller
            while (shouldPage && dsCurrentPage.Tables[0].Rows.Count > 0)  //TODO: More elegant way to check if results are empty than tables[0].Rows.Count?
            {
                ds.Merge(dsCurrentPage);
                pageNum++;
                keypairs["pageNumber"] = pageNum.ToString();
                if (accesstoken != Connector.AccessToken)
                {
                    accesstoken = Connector.AccessToken;
                }
                dsCurrentPage = GetDatasetIteratively(baseUrl, action, accesstoken, accountId, eventId, keypairs);
            }

            //put the first page into the return var if we arent paging since it will be empty otherwise
            if(!shouldPage)
            {
                ds = dsCurrentPage;
            }

            //cache the results
            if (ds != null)
                ConnectorCache.StoreData(key, ds);

            return ds;
        }

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
            return getList(baseUrl, accesstoken, accountId, eventId, keypairs, "attendeelist.json");
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
            return getList(baseUrl, accesstoken, accountId, eventId, keypairs, "regsessionlist.json");
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
                            column.Value = getDefaultDate();
                        if (column.Value.ToString().Equals("0000 - 00 - 00", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
                        if (column.Value.ToString().Equals("0000-00-00", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
                        if (column.Value.ToString().Equals("0001-01-01 00:00:00", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
                        if (column.Value.ToString().Equals("0001 - 01 - 01", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
                        if (column.Value.ToString().Equals("0001-01-01", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
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

        private static String getDefaultDate()
        {
            return String.Empty;
            //return System.DateTime.MinValue.ToString("yyyy-MM-dd");
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
        private static HttpResponse DoHttpGetInternalBase(string baseUrl, string action, string accesstoken, string accountId = null,
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

            http.Request.Timeout = 60000000;
            Log.Debug("Request: {path}", path);
            var result = http.Get(path);

            return result;
        }

        //wraps DoHttpGetInternalBase checking for expired accesstokens and re-invoking as needed
        private static HttpResponse DoHttpGetInternal(string baseUrl, string action, string accesstoken, string accountId = null,
            string eventId = null, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null, string additionCondition = null,
            int? pageSize = null, Dictionary<string, string> keypairs = null)
        {
            var brk = false;
            var result = DoHttpGetInternalBase(baseUrl, action, accesstoken, accountId, eventId, modifiedAfter, modifiedBefore, additionCondition, pageSize, keypairs);
            //check if we need to get a new acccesstoken
            if (result.RawText.StartsWith("{\"status\":\"error\",\"msg\":\"Not authorized to access account") || brk)
            {
                Log.Debug("Access Token expired: {token} .", accesstoken);
                //reauthenticate
                Connector.AccessToken = DataServicesClient.Authorize(Connector.BaseUrl, Connector.AccountId, Connector.ApiKey);
                Log.Debug("New Access Token: {token}", Connector.AccessToken);
                result = DoHttpGetInternalBase(baseUrl, action, Connector.AccessToken, accountId, eventId, modifiedAfter, modifiedBefore, additionCondition, pageSize, keypairs);
            }
            return result;
        }

    }


    #endregion


}


