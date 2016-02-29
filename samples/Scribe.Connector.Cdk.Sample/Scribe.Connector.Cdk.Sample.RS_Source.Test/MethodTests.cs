using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Cryptography;
using Scribe.Core.ConnectorApi.Exceptions;

namespace Scribe.Connector.Cdk.Sample.RS_Source.Test
{
    [TestClass]
    public class MethodTests
    {
        private static readonly IConnector _rsSourceConnector = new RS_SourceConnector();
        private const string InvalidPropertyValue = "xxxxxx";

        /// <summary>
        /// Decryption key for securly passing sensitive data to the connector
        /// Note: this must be that same one both sides
        /// </summary>
        private static string CryptoKey
        {
            get { return "25650FCD-131E-4ECB-AD52-8180BE6D779D"; }
        }

        /// <summary>
        /// Prior to running any tests set the connection properties,
        /// perform the connect method in the connector,
        /// and set the metadata provider varible
        /// </summary>
        [ClassInitialize]
        public static void Startup(TestContext context)
        {
            var connectionProperties = new Dictionary<string, string>();
            //setup the initial parameters for data connection
            connectionProperties.Add("Provider", "SQLNCLI10");
            connectionProperties.Add("Server", "localhost");
            connectionProperties.Add("Database", "ScribeSampleRSSource");
            connectionProperties.Add("UserName", "sa");
            //encrypt the connection password using the shared key
            string encryptedPassword = Encryptor.Encrypt_AesManaged("sa", CryptoKey);
            connectionProperties.Add("Password", encryptedPassword);
            //open a new connection to the datasource
            _rsSourceConnector.Connect(connectionProperties);

        }
        #region Get Object Definition/List Tests
        /// <summary>
        /// This is a validity test to check that we can correctly get a list of 
        /// table names and primary key names from the database
        /// </summary>
        [TestMethod]
        public void GetObjectDefinitionListValidTest()
        {

            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetObjectDefinitionList" };

            //execute the selected method and set the method result
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);

            //since we are requesting a list of Table definitions we want to ensure that everything 
            //went as planned by check that the properties have been returned
            Assert.AreNotSame(methodResult.Return.Properties.Count, 0);
        }


        /// <summary>
        /// This is a validity test to check that we can correctly get a 
        /// specific tables metadata information
        /// </summary>
        [TestMethod]
        public void GetObjectDefinitionValidTest()
        {

            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetObjectDefinition" };

            //Add an input property for the 'ObjectName' that is being requested
            methodInput.Input.Properties.Add("ObjectName", "Addresses");

            //execute the selected method and set the method result
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);

            //since we are requesting a list of Table definitions we want to ensure that everything 
            //went as planned by check that the properties have been returned
            Assert.AreNotSame(methodResult.Return.Properties.Count, 0);
        }

        [TestMethod]
        public void GetObjectDefinitionInValidObjectNameTest()
        {

            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetObjectDefinition" };

            //Add an input property for the 'ObjectName' that is being requested
            methodInput.Input.Properties.Add("ObjectName", InvalidPropertyValue);

            //execute the selected method and set the method result
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);

            //since this is not an error that throws an exception success will be 
            //false and the error information will be propery filled out
            Assert.IsFalse(methodResult.Success);
            Assert.IsNotNull(methodResult.ErrorInfo.Number);
        }

        [TestMethod]
        public void GetObjectDefinitionInValidPropertyNameTest()
        {
            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetObjectDefinition" };

            //Note: the 'ObjectName' property has not been set

            //execute the selected method
            MethodResult result = _rsSourceConnector.ExecuteMethod(methodInput);

            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorInfo);
        }
        #endregion

        #region Get Replication Data Method Test
        /// <summary>
        /// This is a validity test to check that we can correctly get a list of data to replicate
        /// </summary>
        [TestMethod]
        public void GetReplicationDataValidTest()
        {

            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetReplicationData" };

            //set the object name by adding the property to the input
            methodInput.Input.Properties.Add("ObjectName", "SalesOrders");

            //set the Last date of syncronization by adding the property to the input
            methodInput.Input.Properties.Add("LastSyncDate", DateTime.Now.AddYears(-5));

            //execute the selected method and set the method result
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);

            Assert.IsTrue(methodResult.Success);
        }

        /// <summary>
        /// This a negative test inputing an invalid date time
        /// </summary>
        [TestMethod]
        public void GetReplicationDataInvalidDateTest()
        {
            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetReplicationData" };

            //set the object name by adding the property to the input
            methodInput.Input.Properties.Add("ObjectName", "SalesOrders");

            //set the Last date of syncronization by adding the property to the input
            methodInput.Input.Properties.Add("LastSyncDate", InvalidPropertyValue);


            //execute the selected method
            MethodResult result = _rsSourceConnector.ExecuteMethod(methodInput);

            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorInfo);
        }

        /// <summary>
        /// This is an invalid test for requesting an object that does not exist
        /// </summary>
        [TestMethod]
        public void GetReplicationDataInvalidObjectTest()
        {

            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetReplicationData" };

            //set the object name by adding the property to the input
            methodInput.Input.Properties.Add("ObjectName", InvalidPropertyValue);

            //set the Last date of syncronization by adding the property to the input
            methodInput.Input.Properties.Add("LastSyncDate", DateTime.Now.AddYears(-5));


            //execute the selected method
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);

            //Note: this will show it was not a success and set the error info rather than throwing an exception
            Assert.IsFalse(methodResult.Success);
            Assert.IsNotNull(methodResult.ErrorInfo);
        }


        /// <summary>
        /// This is a test for when the 'LastSyncDate' input property is not set
        /// </summary>
        [TestMethod]
        public void GetReplicationDataMissingDatePropertyTest()
        {
            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetReplicationData" };

            //set the object name by adding the property to the input
            methodInput.Input.Properties.Add("ObjectName", "SalesOrders");

            //note: the LastSyncDate input property is not set 


            //execute the selected method and set the method result
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);

            //when the last sync date is missing the result should still be a success
            Assert.IsTrue(methodResult.Success);
        }

        /// <summary>
        /// This is a negative test for when the 'ObjectName' input property is not set
        /// </summary>
        [TestMethod]
        public void GetReplicationDataMissingObjectNamePropertyTest()
        {
            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetReplicationData" };

            //note: the object name input property has not been set

            //set the Last date of syncronization by adding the property to the input
            methodInput.Input.Properties.Add("LastSyncDate", DateTime.Now.AddYears(-5));


            //execute the selected method and set the method result
            MethodResult result = _rsSourceConnector.ExecuteMethod(methodInput);

            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorInfo);
        }


        /// <summary>
        /// This is a validity test to check that we can correctly get a list of data to replicate
        /// </summary>
        [TestMethod]
        public void GetReplicationDataDateOutOfRangeInvalidTest()
        {

            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetReplicationData" };

            //set the object name by adding the property to the input
            methodInput.Input.Properties.Add("ObjectName", "SalesOrders");

            //set the Last date of syncronization by adding the property to the input
            methodInput.Input.Properties.Add("LastSyncDate", DateTime.MinValue);

            //execute the selected method
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);

            //check that the result is not a success
            Assert.IsFalse(methodResult.Success);
        }
        #endregion

        #region InitReplication Test
        /// <summary>
        /// This is a Validity test to attempt to create the table needed to keep track of replication changes
        /// Note: the ScribeChangeHistory_Create.sql script must be in the execution folder for this test to validate
        /// </summary>
        [TestMethod]
        public void InitReplicationValidTest()
        {
            //create a new method input
            MethodInput methodInput = new MethodInput { Name = "InitReplication" };
            //execute the selected method
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);
            //check that the result is a success
            Assert.IsTrue(methodResult.Success);
        }

        #endregion

        #region InitReplicationObject Test
        /// <summary>
        /// This is a positive test for create a trigger on a specified table.
        /// Note: the ScribeDelete_Trigger.sql script must be in the execution folder for this test to validate
        /// </summary>
        [TestMethod]
        public void InitReplicationObjectValidTest()
        {
            //create a new method input
            MethodInput methodInput = new MethodInput { Name = "InitReplicationObject" };

            methodInput.Input.Properties.Add("EntityName","Addresses");
            //execute the selected method
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);
            //check that the result is a success
            Assert.IsTrue(methodResult.Success);
        }

        /// <summary>
        /// This is a negative test for an invalid table name
        /// Note: the ScribeDelete_Trigger.sql script must be in the execution folder for this test to validate
        /// </summary>
        [TestMethod]
        public void InitReplicationObjectInValidObjectNameTest()
        {
            //create a new method input
            MethodInput methodInput = new MethodInput { Name = "InitReplicationObject" };
            //note the invalid entity name
            methodInput.Input.Properties.Add("EntityName", InvalidPropertyValue);
            //execute the selected method
            MethodResult result =  _rsSourceConnector.ExecuteMethod(methodInput);
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorInfo);
        }
        #endregion

        #region GetChangHistoryData Method Tests
        /// <summary>
        /// This is a validty test to ensure that after a change has been made to a table
        /// the changes will be logged and returned in this method execution
        /// </summary>
        [TestMethod]
        public void GetChangeHistoryDataValidTest()
        {
            //create a new method input
            MethodInput methodInput = new MethodInput {Name = "GetChangeHistoryData"};
            //set the method input properties
            methodInput.Input.Properties.Add("ObjectName", "Addresses");
            methodInput.Input.Properties.Add("LastSyncDate", DateTime.Now.AddDays(-10));
            //execute the selected method
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);
            //verify the method was a success
            Assert.IsTrue(methodResult.Success);
        }

        /// <summary>
        /// This is a negative test to ensure that the method execution will still return a success even if 
        /// it does not find the object specified, but will not return any data
        /// </summary>
        [TestMethod]
        public void GetChangeHistoryDataInValidObjectTest()
        {
            //create a new method input
            MethodInput methodInput = new MethodInput { Name = "GetChangeHistoryData" };
            //set the method input properties
            //note: the invalid object name
            methodInput.Input.Properties.Add("ObjectName", InvalidPropertyValue);
            methodInput.Input.Properties.Add("LastSyncDate", DateTime.Now.AddDays(-1));
            //execute the selected method
            MethodResult methodResult = _rsSourceConnector.ExecuteMethod(methodInput);
            //verify the method was a success
            Assert.IsTrue(methodResult.Success);
            //verify that data was not returned
            Assert.IsNotNull(methodResult.Return);
        }

        /// <summary>
        /// This is a negative test to ensure that the method execution will still 
        /// fail if no object name is specified
        /// </summary>
        [TestMethod]
        public void GetChangeHistoryDataInValidObjectNamePropertyTest()
        {
            //create a new method input
            MethodInput methodInput = new MethodInput { Name = "GetChangeHistoryData" };
            //set the method input properties
            //note: the object name property is missing
            methodInput.Input.Properties.Add("LastSyncDate", DateTime.Now.AddDays(-1));
            //execute the selected method
            MethodResult result = _rsSourceConnector.ExecuteMethod(methodInput);
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorInfo);
        }

        /// <summary>
        /// This is a negative test to ensure that the method execution will fail if a
        /// last sync data is in an incorrect format
        /// </summary>
        [TestMethod]
        public void GetChangeHistoryDataInValidDateTest()
        {
            //create a new method input
            MethodInput methodInput = new MethodInput { Name = "GetChangeHistoryData" };
            //set the method input properties
            methodInput.Input.Properties.Add("ObjectName", "Addresses");
            //note: the invalid date
            methodInput.Input.Properties.Add("LastSyncDate", InvalidPropertyValue);
            //execute the selected method
            MethodResult result = _rsSourceConnector.ExecuteMethod(methodInput);
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorInfo);
        }

        /// <summary>
        /// This is a negative test to ensure that the method execution will fail if no date is specified
        /// </summary>
        [TestMethod]
        public void GetChangeHistoryDataInValidPropertyTest()
        {
            //create a new method input
            MethodInput methodInput = new MethodInput { Name = "GetChangeHistoryData" };
            //set the method input property
            methodInput.Input.Properties.Add("ObjectName", "Addresses");

            //note: this will fail because no LastSyncDate is set

            //execute the selected method
            MethodResult result = _rsSourceConnector.ExecuteMethod(methodInput);
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorInfo);
        }

        #endregion
    }
}
