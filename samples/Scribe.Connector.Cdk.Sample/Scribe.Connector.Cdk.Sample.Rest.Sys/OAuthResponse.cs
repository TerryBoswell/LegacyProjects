// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OAuthResponse.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys
{
	/// <summary>
	/// Represense the OAuth Response from GoToWebinar. Used to store the access credentials.
	/// </summary>
	[DataContract]
	public class OAuthResponse
	{

		[DataMember(Name = "access_token")]
		public string AccessToken { get; set; }

		[DataMember(Name = "expires_in")]
		public string ExpiresIn { get; set; }

		[DataMember(Name = "refresh_token")]
		public string RefreshToken { get; set; }

		[DataMember(Name = "organizer_key")]
		public string OrganizerKey { get; set; }

		[DataMember(Name = "account_key")]
		public string AccountKey { get; set; }

	}
}
