// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GoToWebinarClient.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using Scribe.Connector.Cdk.Sample.Rest.Sys.Entities;
using System.Reflection;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys
{

	/// <summary>
	/// Used to connect to Citrix GoToWebinar Rest service, handles OAuth and CRUD operations on the entities
	/// </summary>
	public class GoToWebinarClient : IGoToWebinarClient
	{

		//REST strings: 
		internal const string OAuthHeader = "OAuth oauth_token={0}";

		/// <summary>
		/// The create registrant uri.
		/// </summary>
		internal const string CreateRegistrantUri =
				"https://api.citrixonline.com/G2W/rest/organizers/{0}/webinars/{1}/registrants";

		/// <summary>
		/// uri to authorize with OAuth
		/// </summary>
		internal const string OAuthExchange =
			"https://api.citrixonline.com/oauth/access_token?grant_type=authorization_code&code={0}&client_id={1}";

		/// <summary>
		/// uri to get historical webinars with the given date range
		/// </summary>
		internal const string HistoricalWebinarsUri =
						"https://api.citrixonline.com/G2W/rest/organizers/{0}/historicalWebinars?fromTime={1}&toTime={2}";

		/// <summary>
		/// uri to retreive registrants
		/// </summary>
		internal const string RegistrantsUri =
						"https://api.citrixonline.com/G2W/rest/organizers/{0}/webinars/{1}/registrants";

		/// <summary>
		/// uri to retreive a webinar
		/// </summary>
		internal const string WebinarUri = "https://api.citrixonline.com/G2W/rest/organizers/{0}/webinars/{1}";

		/// <summary>
		/// uri to retrieve all open webinars based on an organizer's key
		/// </summary>
		internal const string UpcomingWebinarsUri =
						"https://api.citrixonline.com/G2W/rest/organizers/{0}/upcomingWebinars";


		/// <summary>
		/// Authenticates the user with the received token and API key.
		/// Hands back a Dictionary of properties extracted from the REST's response.
		/// </summary>
		/// <param name="oAuthCode">oAuth code received from the initial request</param>
		/// <param name="oAuthApiKey">Your Citrix Online assigned API key</param>
		/// <returns>string dictionary containing properties extracted from the OAuthentication jSon response</returns>
		public IDictionary<string, string> Authenticate(string oAuthCode, string oAuthApiKey)
		{
			//construct the REST uri:
			string authUri = string.Format(OAuthExchange, oAuthCode, oAuthApiKey);
			var properties = new Dictionary<string, string>();
			
			//call GoToWebinar's rest api with some reusable code: 
			var oAuthResponse = CallGoToWebinarApi<OAuthResponse>(authUri);

			//Add the REST's response items to a dictionary so we can use them back in 'Connect'
			properties.Add("AccessToken", oAuthResponse.AccessToken);
			properties.Add("AccountKey", oAuthResponse.AccountKey);
			properties.Add("ExpiresIn", oAuthResponse.ExpiresIn);
			properties.Add("OrganizerKey", oAuthResponse.OrganizerKey);
			properties.Add("RefreshToken", oAuthResponse.RefreshToken);

			return properties;
		}

		/// <summary>
		/// Creates a new Registrant in the specified Webinar
		/// </summary>
		/// <param name="accessToken"></param>
		/// <param name="organizerKey"></param>
		/// <param name="webinarKey"></param>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <param name="email"></param>
		public Scribe.Core.ConnectorApi.DataEntity CreateRegistrant(string accessToken, string organizerKey, string webinarKey,
			string firstName, string lastName, string email)
		{

			//create a registrant request. Used for jSon serialization:
			var registrantRequest = new RegistrantRequest
			{
				Email = email,
				FirstName = firstName,
				LastName = lastName
			};

			var uri = string.Format(CreateRegistrantUri, organizerKey, webinarKey);

			//post the data to GoToWebinar:
			//This returns an ID and a JoinUrl. Let's hand those up to Scribe OnLine
			var response = PostGoToWebinarApi<RegistrantRequest, RegistrantResponse>(accessToken, uri, registrantRequest);

			//Want to hand back a DataEntity so we can append it to the OperationRequest:
			var fields = GetEntityFields(response);
			var dataEntity = new Scribe.Core.ConnectorApi.DataEntity
			{
				ObjectDefinitionFullName = "Registrant",
				Properties = new Core.ConnectorApi.Query.EntityProperties()
			};

			foreach (var field in fields)
			{
				dataEntity.Properties.Add
					(
						field.Name,
						field.GetValue(response, null)
					);
			}

			return dataEntity;

		}

		/// <summary>
		/// Calls GoToWebinar's API and retrieves the list of registrants associated with the selected webinar key
		/// </summary>
		/// <param name="accessToken"></param>
		/// <param name="organizerKey"></param>
		/// <param name="webinarKey"></param>
		/// <returns></returns>
		public List<Registrant> GetRegistrants(string accessToken, string organizerKey, string webinarKey)
		{
			//Uses our existing credentials to hit GoToWebinar's REST API and get a list of Registrants:
			var registrantRestUri = string.Format(RegistrantsUri, organizerKey, webinarKey);
			var registrantResponses = CallGoToWebinarApi<List<RegistrantResponse>>(accessToken, registrantRestUri);
			var registrants = new List<Registrant>();

			//convert these to the correct entity and hand them back: 
			foreach (var response in registrantResponses)
			{
				registrants.Add(
					new Registrant(response));
			}

			return registrants.ToList();

		}

		/// <summary>
		/// Gets a list of upcoming webinars assigned to the organizer
		/// </summary>
		/// <param name="accessToken"></param>
		/// <param name="organizerKey"></param>
		/// <param name="webinarKey"></param>
		/// <returns></returns>
		public List<UpcomingWebinar> GetUpcomingWebinars(string accessToken, string organizerKey)
		{
			var upcomingWebinarRestUri = string.Format(UpcomingWebinarsUri, organizerKey);
			var upcomingWebinarResponses = CallGoToWebinarApi<List<WebinarResponse>>(accessToken, upcomingWebinarRestUri);
			var upcomingWebinars = new List<UpcomingWebinar>();

			foreach (var response in upcomingWebinarResponses)
			{
				upcomingWebinars.Add(
					new UpcomingWebinar(response));
			}

			return upcomingWebinars;

		}

		/// <summary>
		/// Gets a list of historical webinars assigned to the organizer
		/// </summary>
		/// <param name="accessToken"></param>
		/// <param name="organizerKey"></param>
		/// <param name="fromDate"></param>
		/// <param name="toDate"></param>
		/// <returns></returns>
		public List<Webinar> GetWebinars(string accessToken, string organizerKey, DateTime fromDate, DateTime toDate)
		{
			//Get the date format that GoToWebinar needs: 
			var fromDateString = string.Format("{0}Z", fromDate.ToString("o").Split('.')[0]);
			var toDateString = string.Format("{0}Z", toDate.ToString("o").Split('.')[0]);
			var getWebinarsUri = string.Format(HistoricalWebinarsUri, organizerKey,
				fromDateString, toDateString);

			var webinarResponses = CallGoToWebinarApi<List<WebinarResponse>>(accessToken, getWebinarsUri);
			var webinars = new List<Webinar>();

			foreach (var response in webinarResponses)
			{
				webinars.Add(
					new Webinar(response));
			}

			return webinars.ToList();

		}

		/// <summary>
		/// Gets a specific webinar based on its key
		/// </summary>
		/// <param name="accessToken"></param>
		/// <param name="organizerKey"></param>
		/// <param name="webinarKey"></param>
		/// <returns></returns>
		public Webinar GetWebinar(string accessToken, string organizerKey, string webinarKey)
		{

			var webinarRestUri = string.Format(WebinarUri, organizerKey, webinarKey);
			var webinarResponse = CallGoToWebinarApi<WebinarResponse>(accessToken, webinarRestUri);
			var webinar = new Webinar(webinarResponse);

			return webinar;

		}

		/// <summary>
		/// A Generic Method to call to the REST service. Hands back the requested response type after 
		/// deserializing the jSon object.
		/// For more documentation on Generic Methods, see: 
		/// http://msdn.microsoft.com/en-us/library/twcad0zb(v=vs.100).aspx
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public T CallGoToWebinarApi<T>(string uri)
		{
			return CallGoToWebinarApi<T>(string.Empty, uri);
		}

		/// <summary>
		/// A Generic Method to call the REST service. Hands back the requested response type after 
		/// deserializing the jSon object.
		/// For more documentation on Generic Methods, see: 
		/// http://msdn.microsoft.com/en-us/library/twcad0zb(v=vs.100).aspx
		/// </summary>
		/// <param name="accessToken">The encrypted oAuth access token as delivered by GoToWebinar</param>
		/// <param name="uri">The REST uri to execute</param>
		/// <returns>Returns the input type from the calling method</returns>
		public T CallGoToWebinarApi<T>(string accessToken, string uri)
		{

			var request = WebRequest.Create(uri) as HttpWebRequest;
			request.Method = "GET";
			request.ContentType = "application/json";

			//create a generic object to hand back: 
			var responseObject = default(T);

			//if there is not an accessToken, then this is an auth request:
			if (!string.IsNullOrEmpty(accessToken))
			{
				request.Headers.Add("Authorization", string.Format(OAuthHeader, accessToken));
			}
			
			using (var response = request.GetResponse() as HttpWebResponse)
			{
				//Check the status code:
				if (response.StatusCode == HttpStatusCode.OK)
				{
					//Deserialize the json and hand back the object: 
					var jsonSerializer = new DataContractJsonSerializer(typeof(T));
					responseObject = (T)jsonSerializer.ReadObject(response.GetResponseStream());
				}
				else
				{
					//TODO: Handle HttStatusCode != OK
				}
					
			}

			return responseObject;

		}

		/// <summary>
		/// A Generic Method to POST to the REST service. Will serialize T data into jSon. 
		/// Generic types may have to be explicitly defined when calling the method. eg "PostGoToWebinarApi<request, response>(accessToken, uri, data)"
		/// </summary>
		/// <typeparam name="T">Request to SEND to the REST service</typeparam>
		/// <typeparam name="U">Response from the REST service.</typeparam>
		/// <param name="accessToken"></param>
		/// <param name="uri"></param>
		/// <param name="data"></param>
		public U PostGoToWebinarApi<T, U>(string accessToken, string uri, T data)
		{

			try
			{

				//create an HttpRequest: 
				var request = WebRequest.Create(uri) as HttpWebRequest;
				request.Method = "POST";
				request.ContentType = "application/json";
				if (!string.IsNullOrEmpty(accessToken))
				{
					request.Headers.Add("Authorization", string.Format(OAuthHeader, accessToken));
				}

				//serialize our content and add it to the request: 
				var jsonSerializer = new DataContractJsonSerializer(typeof(T));
				jsonSerializer.WriteObject(request.GetRequestStream(), data);

				//set up the call: 
				var auth = string.Format("Authorization: " + OAuthHeader, accessToken);

				//execute the call
				using (var response = request.GetResponse() as HttpWebResponse)
				{
					//check the status of the response: 
					if (response != null && response.StatusCode != HttpStatusCode.Created)
					{
						throw new WebException("Unable to create entity " + response.StatusDescription);
					}

					//deserialize the response and send it back. 
					//This allows us to append the key that GoToWebinar generated for the Registrant
					var jsonDeserializer = new DataContractJsonSerializer(typeof(U));
					var responseObject = (U)jsonDeserializer.ReadObject(response.GetResponseStream());

					return responseObject;
				}

			}
			catch (Exception ex)
			{
				throw new WebException("Unable to create entity", ex.InnerException); 
			}

		}

		/// <summary>
		/// Gets a list of fields from the input entity
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <returns></returns>
		private PropertyInfo[] GetEntityFields<T>(T entity)
		{
			var type = typeof(T);
			var properties = type.GetProperties(BindingFlags.Instance |
				BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.GetProperty);

			return properties;

		}
	
	}
}
