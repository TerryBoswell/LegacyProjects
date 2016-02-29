// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGoToWebinarClient.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scribe.Connector.Cdk.Sample.Rest.Sys.Entities;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys
{

	/// <summary>
	/// An Interface for the GoToWebinarClient class
	/// </summary>
	public interface IGoToWebinarClient
	{
		
		IDictionary<string, string> Authenticate(string oAuthCode, string oAuthApiKey);

		Scribe.Core.ConnectorApi.DataEntity CreateRegistrant(string accessToken, string organizerKey, string webinarKey, string firstName,
			string lastName, string email);

		List<Registrant> GetRegistrants(string accessToken, string organizerKey, string webinarKey);

		List<UpcomingWebinar> GetUpcomingWebinars(string accessToken, string organizerKey);

		List<Webinar> GetWebinars(string accessToken, string organizerKey, DateTime fromDate, DateTime toDate);

		Webinar GetWebinar(string accessToken, string organizerKey, string webinarKey);

		T CallGoToWebinarApi<T>(string uri);

		T CallGoToWebinarApi<T>(string accessToken, string uri);

		U PostGoToWebinarApi<T, U>(string accessToken, string uri, T data);

	}

}
