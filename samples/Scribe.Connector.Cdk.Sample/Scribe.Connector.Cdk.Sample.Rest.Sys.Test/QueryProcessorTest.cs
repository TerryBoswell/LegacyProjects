using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Scribe.Core.ConnectorApi.Query;
using Scribe.Connector.Cdk.Sample.Rest.Sys.Entities;
using Scribe.Core.ConnectorApi;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Test
{
	/// <summary>
	/// Uses Moq to test the logic of the QueryProcessor class
	/// </summary>
	[TestClass]
	public class QueryProcessorTest
	{

		/* The goal of these classes is to test the logic of the 
		 * QueryProcessor class.
		 * We don't want the hassle of actually connecting to the REST service
		 * and storing an access token, so we are going to fake it with Moq
		 * All of the testing for the GoToWebinarClient should be done in
		 * its own unit test.
		 */

		[TestMethod]
		public void QueryProcessor_TestExecuteQuery()
		{

			//Uses Moq to test the Query Processor logic
			//Arrange: 
			var connectionInfo = CreateConnectionInfo();
			var query = new Query();
			query.RootEntity.ObjectDefinitionFullName = "Registrant";
			//use this to return a pre-determined set of data that we control: 
			var registrant = CreateRegistrants();
			var webinar = CreateUpcomingWebinars();

			//set up the fake data:
			var clientMoq = new Mock<IGoToWebinarClient>();
			clientMoq.Setup(client => 
				client.GetUpcomingWebinars(connectionInfo["AccessToken"], connectionInfo["OrganizerKey"])).Returns(webinar);
			clientMoq.Setup(client =>
				client.GetRegistrants(connectionInfo["AccessToken"], connectionInfo["OrganizerKey"], "1235813")).Returns(registrant);

			//create our the instance of the queryprocessor using our Moq class:
			var queryProcessor = new QueryProcessor(connectionInfo, clientMoq.Object);

			//Act:
			IEnumerable<DataEntity> dataEntities = queryProcessor.ExecuteQuery(query);

			//Assert:
			//since we're returning a predeternimed list of registrants, 
			//we're only testing the translation of registrants to dataentities.
			Assert.AreEqual(2, dataEntities.Count());
			Assert.IsTrue(dataEntities.Any(entity => 
				entity.Properties.Any(field => field.Value == "jim")));

		}

		[TestMethod]
		public void QueryProcessor_TestExecuteQuery_NullQueryName()
		{

			//Test a 'bad' condition to make sure the code doesn't break: 
			
			//Arrange: 
			var connectionInfo = CreateConnectionInfo();
			var query = new Query();
			query.RootEntity.ObjectDefinitionFullName = null;
			var clientMoq = new Mock<IGoToWebinarClient>();
			var queryProcessor = new QueryProcessor(connectionInfo, clientMoq.Object);

			//Act:
			IEnumerable<DataEntity> dataEntities = queryProcessor.ExecuteQuery(query);

			//Assert:
			Assert.IsNotNull(dataEntities);

		}

		[TestMethod]
		public void QueryProcessor_TestExecuteQuery_UnknownEntityName()
		{

			//Test an unknown entity: 

			//Arrange: 
			var connectionInfo = CreateConnectionInfo();
			var query = new Query();
			query.RootEntity.ObjectDefinitionFullName = "UnkownEntity";
			var clientMoq = new Mock<IGoToWebinarClient>();
			var queryProcessor = new QueryProcessor(connectionInfo, clientMoq.Object);

			//Act:
			IEnumerable<DataEntity> dataEntities = queryProcessor.ExecuteQuery(query);

			//Assert:
			Assert.IsNotNull(dataEntities);

		}

		[TestMethod]
		public void QueryProcessor_TestExecute_FilteredAndOrdered()
		{
			//Arrange:
			var connectionInfo = CreateConnectionInfo();
			var registrants = CreateRegistrants();
			var webinar = CreateUpcomingWebinars();
			var misMatchedRegistrant = new Registrant
			{
				Email = "john@john.com",
				WebinarKey = "246810",
				FirstName = "john"
			};
			registrants.Add(misMatchedRegistrant);
			var query = CreateFilterQuery();

			//set up the fake data:
			var clientMoq = new Mock<IGoToWebinarClient>();
			clientMoq.Setup(client =>
				client.GetUpcomingWebinars(connectionInfo["AccessToken"], connectionInfo["OrganizerKey"])).Returns(webinar);
			clientMoq.Setup(client =>
				client.GetRegistrants(connectionInfo["AccessToken"], connectionInfo["OrganizerKey"], "1235813")).Returns(registrants);
			var queryProcessor = new QueryProcessor(connectionInfo, clientMoq.Object);

			//Act:
			IEnumerable<DataEntity> entities = queryProcessor.ExecuteQuery(query);

			//Assert

			//check the count: 
			Assert.AreEqual(2, entities.Count());

			//check the ordering. We specified Descending, so Jim should be at the top, Bob at the bottom.
			DataEntity jim = entities.First();
			Assert.IsTrue(jim.Properties.Any(field => field.Value == "jim"));

		}

		[TestMethod]
		public void QueryProcessor_TestExecute_DateFilteredWebinar()
		{
			//We're not testing to see if GoToWebinar filters correctly, 
			//but that our Query Builder does the right stuff

			//Arrange
			var connectionInfo = CreateConnectionInfo();
			var query = CreateDateFilterQuery();
			var webinars = CreateWebinars();
			var clientMoq = new Mock<IGoToWebinarClient>();
			
			clientMoq.Setup(client =>
				client.GetWebinars(connectionInfo["AccessToken"], connectionInfo["OrganizerKey"],
				new DateTime(2010, 01, 01), new DateTime(2013, 01, 01))).Returns(webinars);

			var queryProcessor = new QueryProcessor(connectionInfo, clientMoq.Object);

			//Act:
			IEnumerable<DataEntity> entities = queryProcessor.ExecuteQuery(query);

			//Assert
			
			Assert.AreEqual(1, entities.Count());

		}

		#region Private Helpers

		/// <summary>
		/// Creates a reusable 'ConnectionInfo' dictionary for testing:
		/// </summary>
		/// <returns></returns>
		private Dictionary<string, string> CreateConnectionInfo()
		{
			
			var connectionInfo = new Dictionary<string, string>();

			connectionInfo.Add("AccessToken", "1234");
			connectionInfo.Add("OrganizerKey", "5678");

			return connectionInfo;

		}

		/// <summary>
		/// Creates a collection of Registrants for testing
		/// </summary>
		/// <returns></returns>
		private List<Registrant> CreateRegistrants()
		{
			List<Registrant> registrants = new List<Registrant>();

			registrants.Add(

				new Registrant
				{
					Email = "bob@bob.com",
					WebinarKey = "1235813",
					FirstName = "bob"
				});

			registrants.Add(
				new Registrant
				{
					Email = "jim@jim.com",
					WebinarKey = "1235813",
					FirstName = "jim"
				});

			return registrants;
		}

		/// <summary>
		/// Creates the Upcoming Webinar collection for testing
		/// </summary>
		/// <returns></returns>
		private List<UpcomingWebinar> CreateUpcomingWebinars()
		{
			List<UpcomingWebinar> webinars = new List<UpcomingWebinar>();
			//Create a webinar where the Organizer key matches our input from the 'ConnectionInfo' dictionary
			webinars.Add(
				new UpcomingWebinar
				{
					Description = "A Webinar",
					EndTime = DateTime.Now.AddHours(2),
					OrganizerKey = "5678",
					StartTime = DateTime.Now,
					WebinarKey = "1235813"
				});

			return webinars;

		}

		private List<Webinar> CreateWebinars()
		{
			var webinars = new List<Webinar>();
			webinars.Add
				(new Webinar
				{
					EndTime = new DateTime(2012, 01, 02),
					StartTime = new DateTime(2012, 01, 01),
					Description = "This is a webinar",
					WebinarKey = "1235813"
				});

			return webinars;

		}
		
		/// <summary>
		/// Creates a query with a constraint and ordering to filter our result set
		/// </summary>
		/// <returns></returns>
		private Query CreateFilterQuery()
		{
			var query = new Query();
			var constraint = new ComparisonExpression
			{
				ExpressionType = ExpressionType.Comparison,
				LeftValue = new ComparisonValue { Value = "WebinarKey", ValueType = ComparisonValueType.Property },
				RightValue = new ComparisonValue { Value = "1235813", ValueType = ComparisonValueType.Constant }
			};

			var sequence = new Sequence
			{
				PropertyName = "FirstName",
				Direction = SequenceDirection.Descending
			};

			query.Constraints = constraint;
			query.RootEntity.ObjectDefinitionFullName = "Registrant";
			query.RootEntity.SequenceList = new List<Sequence>();
			query.RootEntity.SequenceList.Add(sequence);

			return query;

		}

		private Query CreateDateFilterQuery()
		{

			var query = new Query();
			query.RootEntity.ObjectDefinitionFullName = "Webinar";

			//Build the expressions
			var leftExpression = new ComparisonExpression
			{
				ExpressionType = ExpressionType.Comparison,
				LeftValue = new ComparisonValue { Value = "Webinar.StartTime", ValueType = ComparisonValueType.Property },
				RightValue = new ComparisonValue { Value = new DateTime(2010, 01, 01), ValueType = ComparisonValueType.Property },
				Operator = ComparisonOperator.Equal
			};

			var rightExpression = new ComparisonExpression
			{
				ExpressionType = ExpressionType.Comparison,
				LeftValue = new ComparisonValue { Value = "Webinar.EndTime", ValueType = ComparisonValueType.Property },
				RightValue = new ComparisonValue { Value = new DateTime(2013, 01, 01), ValueType = ComparisonValueType.Property },
				Operator = ComparisonOperator.Equal
			};

			var logical = new LogicalExpression
			{
				ExpressionType = ExpressionType.Logical,
				LeftExpression = leftExpression,
				RightExpression = rightExpression,
				Operator = LogicalOperator.And
			};

			query.Constraints = logical;

			//we're adding the logical expression:
			// StartTime <= 1/1/2010 AND EndTime >= 1/1/2013

			return query;

		}

		#endregion

	}
}
