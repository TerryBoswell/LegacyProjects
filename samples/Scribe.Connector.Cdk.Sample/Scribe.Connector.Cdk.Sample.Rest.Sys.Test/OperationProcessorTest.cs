using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Test
{
	[TestClass]
	public class OperationProcessorTest
	{

		[TestMethod]
		public void ExecuteOperation_Test()
		{
			//Arrange
			var input = BuildOperationInput();
			var connectionInfo = new Dictionary<string, string>();
			connectionInfo.Add("AccessToken", "1234");
			connectionInfo.Add("OrganizerKey", "56789");

			//use a Mock GoToWebinarClient. We're not testing the actual call, but the Operation Code:
			var webinarClient = new Mock<IGoToWebinarClient>();
			var operationProcessor = new OperationProcessor(webinarClient.Object, connectionInfo);
			var entity = BuildEntity();
			webinarClient.Setup(method => method.CreateRegistrant("1234", "56789", "1235813", "Joe", "Josephs", "joe@joe.com")).Returns(entity);

			//Act
			var operationResult = operationProcessor.ExecuteOperation(input);
			
			//Assert: 
			Assert.AreEqual(2, operationResult.Success.Where(result => result == true).Count());

			Assert.IsTrue(operationResult.Output.Any(data => data.ObjectDefinitionFullName == "Registrant"));

		}

		[TestMethod]
		public void ExecuteOperation_TestFailure()
		{

			//Arrange
			var input = BuildOperationInput();
			var connectionInfo = new Dictionary<string, string>();
			connectionInfo.Add("AccessToken", "1234");
			connectionInfo.Add("OrganizerKey", "56789");

			//use a Mock GoToWebinarClient. We're not testing the actual call, but the Operation Code:
			var webinarClient = new Mock<IGoToWebinarClient>();

			//have 'bob' throw an exception:
			webinarClient.Setup(method => method.CreateRegistrant("1234", "56789",
				"1235813", "Bob", "Roberts", "bob@bob.com")).Throws<System.Net.WebException>();
			var operationProcessor = new OperationProcessor(webinarClient.Object, connectionInfo);

			//Act
			var operationResult = operationProcessor.ExecuteOperation(input);

			//Assert: 
			Assert.AreEqual(1, operationResult.Success.Where(result => result == true).Count());
			Assert.AreEqual(1, operationResult.Success.Where(result => result == false).Count());

		}

		#region private helpers

		/// <summary>
		/// Creates a test OperationInput
		/// </summary>
		/// <returns></returns>
		private OperationInput BuildOperationInput()
		{

			var inputs = new DataEntity[2];
			inputs[0] = new DataEntity
			{
				ObjectDefinitionFullName = "Registrant",
				Properties = new Core.ConnectorApi.Query.EntityProperties()
			};

			inputs[1] = new DataEntity
			{
				ObjectDefinitionFullName = "Registrant",
				Properties = new Core.ConnectorApi.Query.EntityProperties()
			};

			inputs[0].Properties.Add("Email", "bob@bob.com");
			inputs[0].Properties.Add("FirstName", "Bob");
			inputs[0].Properties.Add("LastName", "Roberts");
			inputs[0].Properties.Add("WebinarKey", "1235813");

			inputs[1].Properties.Add("Email", "joe@joe.com");
			inputs[1].Properties.Add("FirstName", "Joe");
			inputs[1].Properties.Add("LastName", "Josephs");
			inputs[1].Properties.Add("WebinarKey", "1235813");
			
			var operation = new OperationInput
			{
				Name = "Create",
				Input = inputs
			};

			return operation;

		}

		private DataEntity BuildEntity()
		{

			var entityfields = new Core.ConnectorApi.Query.EntityProperties();
			entityfields.Add("RegistrantKey", "123456");
			entityfields.Add("JoinUrl", "www.scribesoft.com");
			
			return new DataEntity
			{
				ObjectDefinitionFullName = "Registrant",
				Properties = entityfields
			};
		}



		#endregion

	}
}
