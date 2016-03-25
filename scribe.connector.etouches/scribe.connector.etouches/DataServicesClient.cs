using EasyHttp.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scribe.Core.ConnectorApi.Logger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Collections.Concurrent;

namespace Scribe.Connector.etouches
{
    
    public static class DataServicesClient
    {
        #region Reserved Property Names For Rest Calls
        /// <summary>
        /// This is the property we will look for to pass to the lastmodified-lt query parameter in selected values
        /// </summary>
        
        #endregion

        public static string Authorize(string baseUrl, string accountId, string apiKey)
        {
            var http = new HttpClient();
            var uri = new UriBuilder(baseUrl);
            uri.Path = "authenticate"; 

            var result = http.Get(uri.ToString(), new { accountid = accountId, key = apiKey });
            var json = JObject.Parse(result.RawText);
            if (((string)json["status"]).ToLower() == "error")
            {
                Logger.WriteError((string)json["msg"]);
                throw new ApplicationException((string)json["msg"]);
            }
            if (!String.IsNullOrEmpty((string)json["accesstoken"])) 
                return json["accesstoken"].ToString();
            throw new ApplicationException(result.RawText);
        }

        #region Meta Data
        ///eventmetadata/
        public static JObject GetEventMetaData(string baseUrl, string accesstoken, string accountId)
        {
            return DataUtility.GetJObject(baseUrl, Extensions.Actions.EventMeta, accesstoken, accountId);            
        }

        ///sessionmetadata/
        public static JObject GetSessionMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return DataUtility.GetJObject(baseUrl, Extensions.Actions.SessionMeta, accesstoken, accountId, eventId);            
        }

        ///attendeemetadata/
        public static JObject GetAttendeeMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return DataUtility.GetJObject(baseUrl, Extensions.Actions.AttendeeMeta, accesstoken, accountId, eventId);            
        }
        ///regsessionmetadata/        
        public static JObject GetRegSessionMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return DataUtility.GetJObject(baseUrl, Extensions.Actions.RegSessionMeta, accesstoken, accountId, eventId);            
        }

        ///speakermetadata/
        public static JObject GetSpeakerMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return DataUtility.GetJObject(baseUrl, Extensions.Actions.SpeakerMeta, accesstoken, accountId, eventId);
        }

        /*
        GET /sessiontrackmetadata/[accountid]/[eventid] accountid
        */
        public static JObject GetSessionTrackMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return DataUtility.GetJObject(baseUrl, Extensions.Actions.SessionTrackMeta, accesstoken, accountId, eventId);
        }


        /*
        GET /meetingmetadata/[accountid]/[eventid] accountid
        */
        public static JObject GetMeetingMetaData(string baseUrl, string accesstoken, string accountId, string eventId)
        {
            return DataUtility.GetJObject(baseUrl, Extensions.Actions.MeetingMeta, accesstoken, accountId, eventId);
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
        public static DataSet ListEvents(ScribeConnection connection, 
            DateTime? modifiedAfter = null, DateTime? modifiedBefore = null, DateTime? attendeesModifiedAfter = null,
            Dictionary<string, string> keypairs = null)
        {
            string aQuery = string.Empty;
            if (attendeesModifiedAfter.HasValue)
            {
                var d = attendeesModifiedAfter.Value.ToString("yyyy-MM-dd");
                aQuery = $"attendees_modified-gt={d}";
            }
            return DataUtility.GetDataset(connection, Extensions.Actions.Event, null, modifiedAfter, modifiedBefore, 
                aQuery, keypairs);            
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
        public static DataSet ListAttendees(ScribeConnection connection, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null,
            Dictionary<string, string> keypairs = null)
        {
            return DataUtility.GetDataset(connection, Extensions.Actions.Attendee, connection.EventId, 
                modifiedAfter, modifiedBefore, null, keypairs);
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
        public static DataSet ListRegSessions(ScribeConnection connection,
            Dictionary<string, string> keypairs = null)
        {
            return DataUtility.GetDataset(connection, Extensions.Actions.RegSession, connection.EventId, null, null, null, keypairs);
        }

        /*
        /speakerlist/[accountid]/[eventid] *
        deleted (optional)
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListSpeakers(ScribeConnection connection,
            Dictionary<string, string> keypairs = null)
        {
            return DataUtility.GetDataset(connection, Extensions.Actions.Speaker, connection.EventId, null, null, null, keypairs);
        }


        /*
        /sessionlist/[accountid]/[eventid] *
        deleted (optional)
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListSessions(ScribeConnection connection,
            Dictionary<string, string> keypairs = null)
        {
            return DataUtility.GetDataset(connection, Extensions.Actions.Session, connection.EventId, null, null, null, keypairs);
            
        }

        /*
        /sessiontracklist/[accountid]/[eventid] *
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListSessionTracks(ScribeConnection connection,
            Dictionary<string, string> keypairs = null)
        {
            return DataUtility.GetDataset(connection, Extensions.Actions.SessionTrack, connection.EventId, null, null, null, keypairs);
        }

        /*
        /meetinglist/[accountid]/[eventid] *
        pageNumber (optional)
        pageSize (optional)
        */
        public static DataSet ListMeetings(ScribeConnection connection,
            Dictionary<string, string> keypairs = null)
        {
            return DataUtility.GetDataset(connection, Extensions.Actions.Meeting, connection.EventId, null, null, null, keypairs);
        }

        #endregion

        

    }

}
