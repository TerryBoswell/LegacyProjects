// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestConnectorTest.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Core.ConnectorApi;
using Scribe.Connector.Cdk.Sample.Rest.Sys;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Test
{

	/// <summary>
	/// Unit tests for the RestConnector class. 
	/// These classes are built so that we will test all of the private and protected methods just by testing the public ones.
	/// </summary>
	
	[TestClass]
	public class RestConnectorTest
	{

		const string returnUrl = @"http://scribesoft.com/?code={0}";
		const string oauth = "oauth_response";


		/// <summary>
		/// Tests the Connector's Connect method.
		/// You must manually log into their service and let it authorize, then pull the code query string out.
		/// </summary>
		[TestMethod]
		public void RestConnector_ReConnect_Test()
		{
			//Connect twice to retrieve the access token from storage instead of getting it from the authorize call again
			//Will have to manually log into GoToWebinar with a valid account and pull the 'code' off of the URL, paste it below: 
			
			//arrange
			string code = string.Empty;
			
			RestConnector connector = new RestConnector();
			Dictionary<string, string> properties = new Dictionary<string, string>();
			properties.Add(oauth, string.Format(returnUrl, code));

			//act:
			connector.Connect(properties);

			//grab the access token and serialize it. This simulates what Scribe Online does after the inital call to the Connector:
			properties[oauth] = Scribe.Core.ConnectorApi.Serialization.XmlTextSerializer.Serialize<Dictionary<string, string>>(properties, null, null);
			connector.Connect(properties);

			//Assert:
			Assert.IsTrue(connector.IsConnected);

		}

		/// <summary>
		/// Tests the 'Connect' method with an already used 'code'
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(Scribe.Core.ConnectorApi.Exceptions.InvalidConnectionException))]
		public void RestConnector_Connect_TestThrowException()
		{
			// This code query string is expired and will return a 400 Bad Request from GoToWebinar

			//Arrange
			string returnUrl = @"http://scribesoft.com/?code={0}";
			RestConnector connector = new RestConnector();
			Dictionary<string, string> properties = new Dictionary<string, string>();
			properties.Add(oauth, returnUrl);

			//Act:
			connector.Connect(properties);

		}

		/// <summary>
		/// Tests the 'PreConnect' method
		/// </summary>
		[TestMethod]
		public void RestConnector_PreConnect_Test()
		{

			/* Preconnect constructs the URI needed to send the oAuth info
			 * to the REST service. This test will make sure 
			 * that we get the right output.
			 */

			//Arrange:
			const string responseUri = @"https://api.citrixonline.com/oauth/authorize?client_id=fab8d8628f7001650f6a461e83126c73&redirect_uri=http%3a%2f%2fscribesoftware.com";
			RestConnector connector = new RestConnector();
			Dictionary<string, string> properties = new Dictionary<string, string>();
			properties.Add("redirect_uri", "http://scribesoftware.com");

			//Act:
			string oAuthUri = connector.PreConnect(properties);

			//Assert:
			Assert.AreEqual(responseUri, oAuthUri);

		}

		/// <summary>
		/// Tests the 'Disconnect' method
		/// </summary>
		[TestMethod]
		public void RestConnector_Disconnect_Test()
		{

			/* Disconnect disposes of any in-memory objects
			 * and sets 'IsConnected' to false
			 */

			//Arrange: 
			RestConnector connector = new RestConnector();

			//Act:
			connector.Disconnect();

			//Assert: 
			Assert.IsFalse(connector.IsConnected);

		}
	
	}
}
