// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Times.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Runtime.Serialization;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Entities
{
	/// <summary>
	/// Represents the webinar times that come from GoToWebinar.
	/// This class is used to help deserialize those times into a standard .NET DateTime class.
	/// </summary>
	[DataContract]
	public class Times
	{

		[DataMember(Name = "startTime")]
		public string StartTime { get; set; }

		[DataMember(Name = "endTime")]
		public string EndTime { get; set; }

	}
}
