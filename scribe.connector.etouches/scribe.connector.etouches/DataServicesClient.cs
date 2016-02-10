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
        public static DataSet ListEvents(string baseUrl, string accesstoken, string accountId, DateTime? greaterThan = null, DateTime? lessThan = null)
        {
            return GetDataset(baseUrl, "eventlist.json", accesstoken, accountId, null, greaterThan, lessThan);            
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
        public static DataSet ListAttendees(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetDataset(baseUrl, "attendeelist.json", accesstoken, accountId, eventId);            
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
        public static DataSet ListRegSessions(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetDataset(baseUrl, "regsessionlist.json", accesstoken, accountId, eventId);            
        }

        /*
        /speakerlist/[accountid]/[eventid] *
        deleted (optional)
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListSpeakers(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetDataset(baseUrl, "speakerlist.json", accesstoken, accountId, eventId);
        }


        /*
        /sessionlist/[accountid]/[eventid] *
        deleted (optional)
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListSessions(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetDatasetIteratively(baseUrl, "sessionlist.json", accesstoken, accountId, eventId);
        }

        /*
        /sessiontracklist/[accountid]/[eventid] *
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListSessionTracks(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetDatasetIteratively(baseUrl, "sessiontracklist.json", accesstoken, accountId, eventId);
        }

        /*
        /meetinglist/[accountid]/[eventid] *
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListMeetings(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return GetDataset(baseUrl, "meetinglist.json", accesstoken, accountId, eventId);
        }

        #endregion




        #region Utility Methods
        public static HttpClient NewHttpClient(string baseUri = null)
        {
            var http = new HttpClient(baseUri);
            http.LoggingEnabled = true;
            http.StreamResponse = false;
            return http;
        }
        private static DataSet GetDataset(string baseUrl, string action, string accesstoken, string accountId = null,
            string eventId = null, DateTime? greaterThan = null, DateTime? lessThan = null)
        {
            HttpResponse result = DoHttpGetInternal(baseUrl, action, accesstoken, accountId, eventId, greaterThan, lessThan);
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
            string eventId = null)
        {
            HttpResponse result = DoHttpGetInternal(baseUrl, action, accesstoken, accountId, eventId);
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
                        cleanRow.Add(column.Name, column.Value);
                    }
                }

                trgArray.Add(cleanRow);
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(JsonConvert.DeserializeObject<DataTable>(trgArray.ToString()));
            return ds;
        }


        private static HttpResponse DoHttpGetInternal(string baseUrl, string action, string accesstoken, string accountId = null,
            string eventId = null, DateTime? greaterThan = null, DateTime? lessThan = null)
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

            if (lessThan.HasValue)
            {
                var ltDate = lessThan.Value.ToString("yyyy-MM-dd");
                path = $"{path}&lastmodified-lt='{ltDate}'";
            }
            if (greaterThan.HasValue)
            {
                var gtDate = greaterThan.Value.ToString("yyyy-MM-dd");
                path = $"{path}&lastmodified-gt='{gtDate}'";
            }
            //uri.Path = path;
            //return http.Get(uri.ToString(), new { accesstoken = accesstoken });
            return http.Get(path);
        }

       

        #endregion
        

    }

}
