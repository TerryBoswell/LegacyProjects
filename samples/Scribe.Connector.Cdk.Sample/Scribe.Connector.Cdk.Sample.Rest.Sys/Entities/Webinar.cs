// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Webinar.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Linq;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Entities
{
	
	/// <summary>
	/// Represents the GoToWebinar 'Webinar' Entity and its fields
	/// </summary>
	public class Webinar
	{

		/// <summary>
		/// Gets or sets the Description field
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the EndTime field
		/// </summary>
		public DateTime EndTime { get; set; }

		/// <summary>
		/// Gets or sets the OrganizerKey field
		/// </summary>
		public string OrganizerKey { get; set; }

		/// <summary>
		/// Gets or sets the Sessions field
		/// </summary>
		public string Sessions { get; set; }

		/// <summary>
		/// Gets or sets the StartTime field
		/// </summary>
		public DateTime StartTime { get; set; }

		/// <summary>
		/// Gets or sets the Subject field
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// Gets or sets the TimeZone field
		/// </summary>
		public string TimeZone { get; set; }

		/// <summary>
		/// Gets or sets the WebinarKey field.
		/// </summary>
		public string WebinarKey { get; set; }

		/// <summary>
		/// Creates an instance of a Webinar entity
		/// </summary>
		public Webinar() { }

		public Webinar(WebinarResponse webinarResponse)
		{

			this.Description = webinarResponse.Description;
			this.OrganizerKey = webinarResponse.OrganizerKey;
			this.Subject = webinarResponse.Subject;
			this.TimeZone = webinarResponse.TimeZone;
			this.WebinarKey = webinarResponse.WebinarKey;

			this.EndTime = DateTime.Parse(webinarResponse.Times[0].StartTime, 
				null, DateTimeStyles.RoundtripKind);

			this.StartTime = DateTime.Parse(webinarResponse.Times[0].EndTime,
				null, DateTimeStyles.RoundtripKind);

			Sessions = string.Join(",",
				webinarResponse.Times.Select(time =>
					string.Format("[StartTime={0}, EndTime={1}",
					DateTime.Parse(time.StartTime, null, DateTimeStyles.RoundtripKind),
					DateTime.Parse(time.EndTime, null, DateTimeStyles.RoundtripKind))));
				
			//const string sessionFormat = "[StartTime={0}, EndTime={1}]";
			//foreach (var time in webinarResponse.Times)
			//{
			//  this.Sessions += string.Format(sessionFormat,
			//    DateTime.Parse(time.StartTime, null, DateTimeStyles.RoundtripKind),
			//    DateTime.Parse(time.EndTime, null, DateTimeStyles.RoundtripKind));
			//}

		}

	}
}
