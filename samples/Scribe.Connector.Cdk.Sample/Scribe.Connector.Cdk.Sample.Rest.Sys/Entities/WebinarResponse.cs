// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebinarResponse.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Entities
{
	/// <summary>
	/// Creates an instance of the WebinarResponse class. 
	/// Used to deserialize the Webinar and UpcomingWebinar data
	/// </summary>
	[DataContract]
	public class WebinarResponse
	{

		[DataMember(Name = "description")]
		public string Description { get; set; }

		[DataMember(Name = "organizerKey")]
		public string OrganizerKey { get; set; }

		[DataMember(Name = "subject")]
		public string Subject { get; set; }

		[DataMember(Name = "times")]
		public IList<Times> Times { get; set; }

		[DataMember(Name = "timeZone")]
		public string TimeZone { get; set; }

		[DataMember(Name = "webinarKey")]
		public string WebinarKey { get; set; }


	}
}
