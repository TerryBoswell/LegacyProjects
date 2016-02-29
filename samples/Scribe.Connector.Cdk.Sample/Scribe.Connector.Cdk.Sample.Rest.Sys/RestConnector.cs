// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestConnector.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Runtime.Serialization;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.ConnectionUI;
using Scribe.Core.ConnectorApi.Cryptography;
using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Core.ConnectorApi.Serialization;
using Scribe.Core.ConnectorApi.Query;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys
{

	/// <summary>
	/// Implements IConnector from the CDK. This is the main entry point to and from Scribe Online.
	/// </summary>
	[ScribeConnector(ConnectorTypeIdAsString, ConnectorTypeName, ConnectorTypeDescription, typeof(RestConnector),
		SettingsUITypeName, SettingsUIVersion, ConnectionUITypeName, ConnectionUIVersion, XapFileName,
		new[] { "Scribe.IS.Target", "Scribe.IS.Source" }, SupportsCloud, "1.0.0.0")]
	public class RestConnector : IConnector
	{

		#region Class Members 

		private MetadataProvider _metadataProvider;
		private IGoToWebinarClient _webinarClient;
		internal IDictionary<string, string> _connectionInfo = new Dictionary<string, string>();

		private static string CryptoKey
		{
			//This value is used to encrypt/decrypt the access token.
			//Generate a new GUID for your connector
			get { return "FEBBB2D0-3ABF-4B15-A0DE-7C6EE5FEDC1B"; }
		}
			
		//Keys needed for oAuth:
		internal const string OAuthRedirectUriKey = "redirect_uri";
		internal const string OAuthResponse = "oauth_response";
		internal const string OAuthUrlFormat = "https://api.citrixonline.com/oauth/authorize?client_id={0}&redirect_uri={1}";

		/// <summary>
		/// This API key is assigned to you by Citrix when you sign up for a developer's account.
		/// </summary>
		private string OAuthApiKey = string.Empty;

		/// <summary>
		/// Gets or sets the QueryProcessor. Used to handle incoming query requests from Scribe OnLine.
		/// </summary>
		protected QueryProcessor QueryProcessor { get; set; }
		protected OperationProcessor OperationProcessor { get; set; }

		#endregion

		#region Members used by core for reflection

		/// <summary>
		///   This constant holds the setting representing the name of the UI type.
		/// </summary>
		internal const string SettingsUITypeName = "";

		/// <summary>
		///   This constant holds the setting for UI version.
		/// </summary>
		internal const string SettingsUIVersion = "1.0";

		/// <summary>
		///   This constant holds the name of the UI connection type. 
		///   Scribe Online has a generic UI for OAuth connections. Use this namespace to call that UI.
		/// </summary>
		internal const string ConnectionUITypeName = "ScribeOnline.Views.OAuthConnectionUI";

		/// <summary>
		///   This constant holds the version of the UI connection.
		/// </summary>
		internal const string ConnectionUIVersion = "1.0";

		/// <summary>
		///   This constant holds the name of the xap file.
		/// </summary>
		internal const string XapFileName = "ScribeOnline";

		/// <summary>
		///   This constant holds this Connector Type Id, unique to this connector.
		///   You will have to generate a new GUID for your connector and insert it here.
		/// </summary>
		internal const string ConnectorTypeIdAsString = "6CF7D8B3-EAFE-456F-B6D4-2FC63BB4301B";

		/// <summary>
		///   The name of the Connector Type.
		/// </summary>
		internal const string ConnectorTypeName = "CDK REST Sys Sample";

		/// <summary>
		///   The description of the Connector Type.
		/// </summary>
		internal const string ConnectorTypeDescription = "Cdk REST SYS Sample from Scribe.";

		/// <summary>
		/// Default metadata file name prefix
		/// </summary>
		internal const string MetadataPrefix = "RestSYSSample";

		internal const bool SupportsCloud = true;

		#endregion

		public RestConnector() 
		{
			_metadataProvider = new MetadataProvider();
			_webinarClient = new GoToWebinarClient();
		}

		#region IConnector Implementation

		//IConnect Properties
		public bool IsConnected { get; set; }

		public Guid ConnectorTypeId
		{
			get { return new Guid(ConnectorTypeIdAsString); }
		}

		//IConnect Methods

		/// <summary>
		/// Returns the correct REST url needed for authorization
		/// </summary>
		/// <param name="properties">Collection of information required by this Connector to connect to the REST service.</param>
		/// <returns>String URL directing the end user to log into GoToWebinar. 
		/// The URL will look like:https://api.citrixonline.com/oauth/authorize?client_id=YourApiKeyHere&redirect_uri=ScribeOnlineUrl
		/// where the redirect query parameter is percent encoded.</returns>
		public string PreConnect(IDictionary<string, string> properties)
		{
			
			/* Simply builds the REST url from method properties */
			return string.Format(OAuthUrlFormat, OAuthApiKey, HttpUtility.UrlEncode(properties[OAuthRedirectUriKey]));

		}

		/// <summary>
		/// Connects the Connector and authorizes the credentials of the user.
		/// </summary>
		/// <param name="properties">Collection of information required by this Connector to connect to the REST service.</param>
		public void Connect(IDictionary<string, string> properties)
		{

			/* LogMethodExecution will write time stamps at the start and
			 * end of the using statement. Use this to keep track of when 
			 * methods are executing
			 */

			using (new LogMethodExecution("Rest CDK Example", "RestConnector.Connect()"))
			{

				/* For the initial authorization, we need to connect and get the access token
				 * Pull the oAuth information out of the properties dictionary and verify its format:
				 */
				try
				{
					if (Uri.IsWellFormedUriString(properties[OAuthResponse], UriKind.RelativeOrAbsolute))
					{
						var queryString = HttpUtility.ParseQueryString(properties[OAuthResponse]);
						var goToWebinar = new GoToWebinarClient();

						string oAuthCode = queryString["code"];

						//get the properties from the REST service:
						_connectionInfo = goToWebinar.Authenticate(oAuthCode, OAuthApiKey);

						//loop through and add them to our dictionary: 
						foreach (var item in _connectionInfo)
						{
							properties.Add(item.Key, item.Value);
						}

						//Use Scribe's CDK to encrypt the access token in the dictionary: 
						//Scribe OnLine uses the properties parameter further up, byref, to store these values. 
						properties["AccessToken"] = Encryptor.Encrypt_AesManaged(properties["AccessToken"], CryptoKey);
					}
					else
					{
						/* We've connected before and already have the token in our dictionary: 
						*  We're using the connection data that's serialized in memory
						*/

						
						using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(properties[OAuthResponse])))
						{

							//Use the CDK to deserialize the properties dictionary
							_connectionInfo = XmlTextSerializer.DeserializeFromStream<Dictionary<string, string>>(memoryStream);

							/*Check the properties for the access token, if it doesn't exist, dig into it for the
							 * oauth_response and look there for the access token
							 */
							if (_connectionInfo.ContainsKey("AccessToken"))
							{
								_connectionInfo["AccessToken"] = Decryptor.Decrypt_AesManaged(_connectionInfo["AccessToken"], CryptoKey);
							}
							else
							{
								if (_connectionInfo.ContainsKey(OAuthResponse))
								{
									using (var innerStream = new MemoryStream(Encoding.UTF8.GetBytes(_connectionInfo[OAuthResponse])))
									{
										_connectionInfo = XmlTextSerializer.DeserializeFromStream<Dictionary<string, string>>(innerStream);
										_connectionInfo["AccessToken"] = Decryptor.Decrypt_AesManaged(_connectionInfo["AccessToken"], CryptoKey);
									}
								}
							}
						}
					}

					//Check to see how the connection went and set some additional members on this class: 
					if (_connectionInfo.Count > 0)
					{

						IsConnected = true;
						this.QueryProcessor = new QueryProcessor(_connectionInfo, _webinarClient);
						this.OperationProcessor = new OperationProcessor(this._webinarClient, this._connectionInfo);

					}

				}
				catch (System.Net.WebException exception)
				{
					//Use the CDK to create a message from the exception
					string message = ExceptionFormatter.BuildActionableException(
						"Unable to connect to the service due to a Web exception",
						"The following error occured in the REST Connector",
						exception);

					//log the exception
					Logger.Write(Logger.Severity.Error, "REST Connector", message);

					//Throw an exception that the CDK will recognize:
					throw new InvalidConnectionException(message);
				}
				catch (Exception exception)
				{
					string message = ExceptionFormatter.BuildActionableException(
						"Unable to connect to the service.",
						"The following error occured while trying to connect to the REST service.",
						exception);

					Logger.Write(Logger.Severity.Error, "REST Connector", message);

					throw new InvalidConnectionException(message);

				}
			}
		}

		/// <summary>
		/// Disconnects the Connector. 
		/// Since REST doesn't stay connected, we just log the event and flip the IsConnected property to false.
		/// </summary>
		public void Disconnect()
		{

			using (new LogMethodExecution("Rest CDK Example", "RestConnector.Disconnect()"))
			{
				IsConnected = false;
			}

		}

		public MethodResult ExecuteMethod(MethodInput input)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Processes the operations requested by Scribe Online
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public OperationResult ExecuteOperation(OperationInput input)
		{

			//Hand the whole request down to the Operation Processor and 
			//Pass the result back to Scribe Online: 

			OperationResult operationResult;

			using (new LogMethodExecution("Rest CDK Example", "Rest.ExecuteOperation()"))
			{

				try
				{
					operationResult = this.OperationProcessor.ExecuteOperation(input);
					LogOperationResults(operationResult);
				}
				catch (Exception ex)
				{
					var message = string.Format("{0} {1}", "Error!", ex.Message);
					Logger.Write(Logger.Severity.Error, "Rest CDK Example", message);
					throw new InvalidExecuteOperationException(message);
				}

			}

			return operationResult;

		}

		/// <summary>
		/// Handles the query request from Scribe Online.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public IEnumerable<DataEntity> ExecuteQuery(Query query)
		{

			using (new LogMethodExecution("Rest CDK Example", "RestConnector.Connect()"))
			{

				IEnumerable<DataEntity> entities = null;

				try
				{
					entities = QueryProcessor.ExecuteQuery(query);
				}
				catch (Exception exception)
				{
					
					//Write the exception to the log: 
					var message = string.Format("{0} {1}", "Adapter Error", exception.Message);
					Logger.Write(Logger.Severity.Error, "Rest CDK Example", message);
					throw new InvalidExecuteQueryException(message);

				}

				return entities;

			}

		}

		/// <summary>
		/// Returns the MetadataProvider
		/// </summary>
		/// <returns></returns>
		public IMetadataProvider GetMetadataProvider()
		{
			return _metadataProvider;
		}

		#endregion

		#region private methods

		private void LogOperationResults(OperationResult operationResult)
		{
			string message;
			Logger.Severity severity;

			//check the result of the operation and write it to the trace log: 
			if (operationResult.Success[0])
			{
				message = "Successful operation";
				severity = Logger.Severity.Debug;
			}
			else 
			{
				//There was an error, but we need to determine what kind: 
				ErrorResult errorInfo = operationResult.ErrorInfo[0];
				if (errorInfo.Number == ErrorNumber.DuplicateUniqueKey)
				{
					//There were duplicate keys, this is considered a warning:
					message = string.Format("{0}{2}Number:{1}{2}Description:{3}{2}",
																						"Warning!", errorInfo.Number,
																						Environment.NewLine,
																						errorInfo.Description);
					severity = Logger.Severity.Warning;
				}
				else
				{
					//there was a fatal error:
					message = string.Format("{0}{2}Error Number:{1}{2}Error Description:{3}{2}Error Detail:{4}",
																						"Error!", errorInfo.Number,
																						Environment.NewLine,
																						errorInfo.Description, errorInfo.Detail);
					severity = Logger.Severity.Error;
				}
			}

			//write the message out to the trace log: 
			Logger.Write(severity, "Rest CDK Example", message);

		}

		#endregion


	}
}
