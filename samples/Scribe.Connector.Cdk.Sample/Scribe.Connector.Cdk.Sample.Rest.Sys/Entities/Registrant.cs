// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Registrant.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using	System.ComponentModel.DataAnnotations;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Entities
{
	/// <summary>
	/// Represents the GoToWebinar 'Registrant' Entity and its fields
	/// </summary>
	public class Registrant
	{

		/// <summary>
		/// Gets or sets the Email field. This field is required.
		/// </summary>
		[Required]
		[Key]
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the FirstName field. This field is required.
		/// </summary>
		[Required]
		public string FirstName { get; set; }

		/// <summary>
		/// Gets or sets the JoinUrl field. Once bound, it is read only. 
		/// </summary>
		[ReadOnly(true)]
		public string JoinUrl { get; set; }

		/// <summary>
		/// Gets or sets the LastName field. This field is required.
		/// </summary>
		[Required]
		public string LastName { get; set; }

		/// <summary>
		/// Gets or sets the RegistrantKey field. Once bound, it is read only.
		/// </summary>
		[ReadOnly(true)]
		public string RegistrantKey { get; set; }

		/// <summary>
		/// Gets or sets the RegistrationDate field. Once bound, it is read only.
		/// </summary>
		[ReadOnly(true)]
		public DateTime RegistrationDate { get; set; }

		/// <summary>
		/// Gets or sets the Status field. Once bound, it is read only.
		/// </summary>
		[ReadOnly(true)]
		public string Status { get; set; }

		/// <summary>
		/// Gets or sets the TimeZone field. Once bound, it is read only.
		/// </summary>
		[ReadOnly(true)]
		public string TimeZone { get; set; }

		/// <summary>
		/// Gets or sets the WebinarKey field
		/// </summary>
		[Required]
		public string WebinarKey { get; set; }

		/// <summary>
		/// Default contructor for the Registrant entity.
		/// </summary>
		public Registrant() { }

		/// <summary>
		/// Creates a Registrant entity from the response
		/// </summary>
		/// <param name="registrantRespons"></param>
		public Registrant(RegistrantResponse registrantResponse)
		{

			this.Email = registrantResponse.Email;
			this.FirstName = registrantResponse.FirstName;
			this.JoinUrl = registrantResponse.JoinUrl;
			this.LastName = registrantResponse.LastName;
			this.RegistrationDate = DateTime.Parse(registrantResponse.RegistrationDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
			this.RegistrantKey = registrantResponse.RegistrantKey;
			this.Status = registrantResponse.Status;
			this.TimeZone = registrantResponse.TimeZone;
			this.WebinarKey = registrantResponse.WebinarKey;

		}

	}
}
