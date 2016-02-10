using EasyHttp.Http;
using Newtonsoft.Json.Linq;
using System;

namespace Scribe.Connector.etouches
{
    
    public static class ApiV2Client
    {
        //private static string baseUrl = "https://eiseverywhere.com";

        public static HttpClient NewHttpClient(string baseUri=null)
        {
            var http = new HttpClient(baseUri);
            http.LoggingEnabled = true;
            http.StreamResponse = false;
            return http;
        }

        public static string Authorize(string baseUrl, string accountId, string apiKey)
        {
            var http = NewHttpClient();
            var uri = new UriBuilder(baseUrl);
            uri.Path = "api/v2/global/authorize.json";

            var result = http.Get(uri.ToString(), new { accountid = accountId, key = apiKey });
            if (result.ContentType.Contains("application/json"))
            {
                var json = JObject.Parse(result.RawText);
                if(json["accesstoken"]!=null)
                    return json["accesstoken"].ToString();
            }

            throw new ApplicationException(result.RawText);
        }

        public static JArray ListEvents(string baseUrl, string accesstoken)
        {
            var http = NewHttpClient();
            var uri = new UriBuilder(baseUrl);
            uri.Path = "api/v2/global/listEvents.json";
            
            var result = http.Get(uri.ToString(), new { accesstoken = accesstoken });
            if (result.ContentType.Contains("application/json"))
            {
                var json = JArray.Parse(result.RawText);
                return json;
            }

            throw new ApplicationException(result.RawText);
        }

        public static JObject GetEvent(string baseUrl, string accesstoken, string eventId)
        {
            var http = NewHttpClient();
            var uri = new UriBuilder(baseUrl);
            uri.Path = "api/v2/ereg/getEvent.json";

            var result = http.Get(uri.ToString(), new { accesstoken = accesstoken, eventid = eventId });
            if (result.ContentType.Contains("application/json"))
            {
                var json = JObject.Parse(result.RawText);
                if (json["error"] == null)
                    return json;
                else
                    throw new ApplicationException(json["error"].ToString());
            }

            throw new ApplicationException(result.RawText);
        }

        public static JObject GetAttendee(string baseUrl, string accesstoken, string eventId, string attendeeId)
        {
            var http = NewHttpClient();
            var uri = new UriBuilder(baseUrl);
            uri.Path = "api/v2/ereg/getAttendee.json";

            var result = http.Get(uri.ToString(), new { accesstoken = accesstoken, eventid = eventId, attendeeid = attendeeId });
            if (result.ContentType.Contains("application/json"))
            {
                var json = JObject.Parse(result.RawText);
                if (json["error"] == null)
                    return json;
                else
                    throw new ApplicationException(json["error"].ToString());
            }

            throw new ApplicationException(result.RawText);
        }

        public static JArray ListAttendees(string baseUrl, string accesstoken, string eventId, string limit="2000", string offset="0", string modifiedFrom=null, string modifiedTo=null)
        {
            var http = NewHttpClient();
            var uri = new UriBuilder(baseUrl);
            uri.Path = "api/v2/ereg/listAttendees.json";

            var result = http.Get(uri.ToString(), new { accesstoken = accesstoken, eventid = eventId, limit = limit, offset = offset, modifiedfrom = modifiedFrom, modifiedto = modifiedTo });
            if (result.ContentType.Contains("application/json"))
            {
                var json = JArray.Parse(result.RawText);
                return json;
            }

            throw new ApplicationException(result.RawText);
        }

        public static JArray ListQuestions(string baseUrl, string accesstoken, string eventId)
        {
            var http = NewHttpClient();
            var uri = new UriBuilder(baseUrl);
            uri.Path = "api/v2/ereg/listQuestions.json";

            var result = http.Get(uri.ToString(), new { accesstoken = accesstoken, eventid = eventId });
            if (result.ContentType.Contains("application/json"))
            {
                var json = JArray.Parse(result.RawText);
                return json;
            }

            throw new ApplicationException(result.RawText);
        }
    
    }

}
