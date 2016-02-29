// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegistrantRequest.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Runtime.Serialization;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Entities
{
	/// <summary>
	/// Creates an instance of the RegistrantRequest class.
	/// Used serialize an insert request for the Registrant entity.
	/// </summary>
	[DataContract]
	public class RegistrantRequest
	{

		[DataMember(Name = "firstName")]
		public string FirstName { get; set; }

		[DataMember(Name = "lastName")]
		public string LastName {get; set;}

		[DataMember(Name = "email")]
		public string Email {get; set;}


	}
}
