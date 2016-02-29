// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryProcessor.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Query;
using Scribe.Connector.Cdk.Sample.Rest.Sys.Entities;
using System.Reflection;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys
{

	/// <summary>
	/// Processes user queries and retrieves data from the REST service
	/// </summary>
	public class QueryProcessor
	{

		#region Members

		private readonly IDictionary<string, string> ConnectionInfo;
		private readonly IGoToWebinarClient webinarClient = new GoToWebinarClient();
		private const string AccessToken = "AccessToken";
		private const string OrganizerKey = "OrganizerKey";

		private DateTime startDate;
		private DateTime endDate;

		#endregion

		#region Constructors

		public QueryProcessor(IDictionary<string, string> ConnectionInfo, IGoToWebinarClient goToWebinarClient)
		{

			this.ConnectionInfo = ConnectionInfo;
			this.webinarClient = goToWebinarClient;

		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Executes the incoming query and hands back a collection of DataEntities.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public IEnumerable<DataEntity> ExecuteQuery(Query query)
		{
			IEnumerable<DataEntity> results = new List<DataEntity>();
			string entityName = query.RootEntity.ObjectDefinitionFullName;
			
			//Route the request to the correct 'Get' in the REST class.
			//Then convert our entity object into a Scribe Online, generic entity

			//GoToWebinar's GetWebinar call allows us to pass in a StartDate and EndDate. 
			//Let's build some logic to let the server do the filtering for us. 
			//This is the PREFERED METHOD since it doesn't take up the client's CPU cycles.

			switch (entityName)
			{

				case "Registrant":
					var registrants = this.GetRegistrants();
					IEnumerable<Registrant> filteredRegistrants = ApplyFilters(registrants, query);
					results = this.ToDataEntity<Registrant>(filteredRegistrants);
					break;

				case "UpcomingWebinar":
					var upcomingWebinars = this.GetUpcomingWebinars();
					IEnumerable<UpcomingWebinar> filteredUpcomingWebinars = ApplyFilters(upcomingWebinars, query);
					results = this.ToDataEntity<UpcomingWebinar>(filteredUpcomingWebinars);
					break;

				case "Webinar":
					//Parse the filtering first: 
					this.startDate = DateTime.MinValue;
					this.endDate = DateTime.UtcNow;
										
					//Recurse through the expressions, looking for user-supplied dates.
					//If they don't exist, we'll use the defaults assigned above.
					this.FindDates(query.Constraints);

					//Go grab the filtered webinars
					var webinars = this.GetWebinars(startDate, endDate);
 					
					//Apply all the user-defined filters. This includes any dates we already
					//filtered on the server, but there's no way to extract them out of the expression
					results = ToDataEntity(ApplyFilters(webinars, query));
					break;

				default:
					break;

			}

			return results;

		}

		#endregion

		#region Private Methods
		
		/// <summary>
		/// Performs the actual 'get' from our REST client for the Registrants
		/// </summary>
		/// <returns></returns>
		private IQueryable<Registrant> GetRegistrants()
		{

			List<Registrant> registrants = new List<Registrant>();

			//Get the associated webinar key first: 
			var webinar = webinarClient.GetUpcomingWebinars(ConnectionInfo[AccessToken], ConnectionInfo[OrganizerKey]);

			if (webinar.Count > 0)
			{
				//As an example we're only getting registrants from the FIRST webinar instead of the whole list.
				var webinarKey = webinar.FirstOrDefault().WebinarKey;
				
				//Now go get the registrants associated with this webinar.
				registrants = webinarClient.GetRegistrants(ConnectionInfo[AccessToken],
					ConnectionInfo[OrganizerKey], webinarKey);
			}
			
			return registrants.AsQueryable();

		}

		/// <summary>
		/// Gets all available webinars for the current organizer
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		private IQueryable<Webinar> GetWebinars(DateTime startDate, DateTime endDate)
		{

			var webinar = webinarClient.GetWebinars(ConnectionInfo[AccessToken], ConnectionInfo[OrganizerKey],
				startDate, endDate);
			return webinar.AsQueryable();

		}

		/// <summary>
		/// Gets the open webinars for the current organizer
		/// </summary>
		/// <returns></returns>
		private IQueryable<UpcomingWebinar> GetUpcomingWebinars()
		{

			var webinars = webinarClient.GetUpcomingWebinars(ConnectionInfo[AccessToken], ConnectionInfo[OrganizerKey]);
			return webinars.AsQueryable();

		}

		/// <summary>
		/// Adds user-requested filters
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entities"></param>
		/// <param name="query"></param>
		/// <returns></returns>
		private IEnumerable<T> ApplyFilters<T>(IQueryable<T> entities, Query query)
		{

			//Use Microsoft's System.Linq.Dynamic library to select the items we want IF there are any filters in the query:
			var results = (query.Constraints != null) ? entities.Where(query.ToLinqExpression()) : entities;

			//order the results if the user has requested it, then send it back:
			return (query.RootEntity.SequenceList.Count > 0) ? results.OrderBy(query.ToOrderByLinqExpression()) : results;

		}

		/// <summary>
		/// Transforms the specified entity collection into a Scribe Online Data Entity collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entities"></param>
		/// <returns></returns>
		private IEnumerable<DataEntity> ToDataEntity<T>(IEnumerable<T> entities)
		{
			//get the type and its properties. We'll use this to build the fields with Reflection
			var type = typeof(T);
			var fields = type.GetProperties(BindingFlags.Instance |
				BindingFlags.FlattenHierarchy |
				BindingFlags.Public |
				BindingFlags.GetProperty);

			//Loop through the retrieved entities and create a DataEntity from it: 
			foreach (var entity in entities)
			{

				var dataEntity = new QueryDataEntity
				{
					ObjectDefinitionFullName = type.Name,
					Name = type.Name
				};

				//Add the fields to the entity:

				/* Each KeyValuePair in the fields must be complete.
				 * If the field's value is NULL here, Scribe OnLine will throw
				 * an exception. 
				 * To send a NULL field to Scribe Online, just don't add it to this dictionary.
				 */

				foreach (var field in fields)
				{
					dataEntity.Properties.Add(
						field.Name,
						field.GetValue(entity, null));
				}

				//Hand back the completed object: 
				yield return dataEntity.ToDataEntity();

			}

		}

		/// <summary>
		/// Recurses through the expressions looking for dates.
		/// </summary>
		/// <param name="expression"></param>
		private void FindDates(Expression expression)
		{

			if (expression is ComparisonExpression)
			{

				//Check for dates, assign if they exist:
				var comparison = (ComparisonExpression)expression;

				if (comparison.LeftValue.Value.ToString() == "Webinar.StartTime"
						&& (comparison.Operator == ComparisonOperator.Equal
								|| comparison.Operator == ComparisonOperator.Greater
								|| comparison.Operator == ComparisonOperator.GreaterOrEqual))
				{
					this.startDate = DateTime.Parse(
						comparison.RightValue.Value.ToString());

				}

				if (comparison.LeftValue.Value.ToString() == "Webinar.EndTime"
						&& (comparison.Operator == ComparisonOperator.Equal
								|| comparison.Operator == ComparisonOperator.Less
								|| comparison.Operator == ComparisonOperator.LessOrEqual))
				{
					this.endDate = DateTime.Parse(
						comparison.RightValue.Value.ToString());
				}
				
			}
			else
			{

				//This is a logical expression. 
				//Recurse through it and keep checking it until we're 
				//Down to ComparisonExpressions

				var logical = (LogicalExpression)expression;
				//If we don't have any filters, this will be null.
				if (logical != null)
				{
					var left = logical.LeftExpression;
					FindDates(left);
					
					var right = logical.RightExpression;
					FindDates(right);
				}
			}

		}
				
		#endregion

	}
}
