// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpcomingWebinar.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Entities
{
	/// <summary>
	/// Represents the GoToWebinar 'UpcomingWebinar' Entity and its fields
	/// </summary>
	public class UpcomingWebinar
	{

		/// <summary>
		/// Gets or sets the Description field
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the EndDate field
		/// </summary>
		public DateTime EndTime { get; set; }

		/// <summary>
		/// Gets or sets the OrganizerKey field
		/// </summary>
		public string OrganizerKey { get; set; }

		/// <summary>
		/// Gets or sets the Subject field
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// Gets or sets the Sessions field
		/// </summary>
		public string Sessions { get; set; }

		/// <summary>
		/// Gets or sets the StartDate field
		/// </summary>
		public DateTime StartTime { get; set; }

		/// <summary>
		/// Gets or sets the TimeZone field
		/// </summary>
		public string TimeZone { get; set; }

		/// <summary>
		/// Gets or sets the WebinarKey field
		/// </summary>
		public string WebinarKey { get; set; }

		/// <summary>
		/// Creates a new instance of the UpcomingWebinar entity
		/// </summary>
		public UpcomingWebinar() { }

		public UpcomingWebinar (WebinarResponse webinarResponse)
		{
			this.Description = webinarResponse.Description;
			this.OrganizerKey = webinarResponse.OrganizerKey;
			this.Subject = webinarResponse.Subject;
			this.TimeZone = webinarResponse.TimeZone;
			this.WebinarKey = webinarResponse.WebinarKey;

			this.EndTime = DateTime.Parse(webinarResponse.Times[0].StartTime,
				null, System.Globalization.DateTimeStyles.RoundtripKind);

			this.StartTime = DateTime.Parse(webinarResponse.Times[0].EndTime,
				null, System.Globalization.DateTimeStyles.RoundtripKind);

			const string sessionFormat = "[StartTime={0}, EndTime={1}]";
			foreach (var time in webinarResponse.Times)
			{
				this.Sessions += string.Format(sessionFormat,
					DateTime.Parse(time.StartTime, null, System.Globalization.DateTimeStyles.RoundtripKind),
					DateTime.Parse(time.EndTime, null, System.Globalization.DateTimeStyles.RoundtripKind));
			}
			
		}
	}
}
