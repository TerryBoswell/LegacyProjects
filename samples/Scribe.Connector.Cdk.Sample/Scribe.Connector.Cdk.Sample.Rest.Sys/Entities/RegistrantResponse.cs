// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegistrantResponse.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Runtime.Serialization;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Entities
{
	/// <summary>
	/// Creates an instance of the RegistrantResponse class.
	/// Used to deserialze the Registrant data from GoToWebinar
	/// </summary>
	[DataContract]
	public class RegistrantResponse
	{

		[DataMember(Name = "email")]
		public string Email { get; set; }

		[DataMember(Name = "firstName")]
		public string FirstName { get; set; }

		[DataMember(Name = "joinUrl")]
		public string JoinUrl { get; set; }

		[DataMember(Name = "lastName")]
		public string LastName { get; set; }

		[DataMember(Name = "registrantKey")]
		public string RegistrantKey { get; set; }

		[DataMember(Name = "registrationDate")]
		public string RegistrationDate { get; set; }

		[DataMember(Name = "status")]
		public string Status { get; set; }

		[DataMember(Name = "timeZone")]
		public string TimeZone { get; set; }

		public string WebinarKey { get; set; }

	}
}
