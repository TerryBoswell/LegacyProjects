// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodTests.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Cryptography;
using Scribe.Core.ConnectorApi.Query;

namespace Scribe.Connector.Cdk.Sample.RS_Target.Test
{
    [TestClass]
    public class MethodTests
    {
        private static readonly IConnector _rsTargetConnector = new RS_TargetConnector();
        private const string InvalidPropertyValue = "xxxxxx";

        /// <summary>
        /// Decryption key for securly passing sensitive data to the connector
        /// Note: this must be that same one both sides
        /// </summary>
        private static string CryptoKey
        {
            get { return "1E093A29-FBA8-4534-9EBD-AA7310477A70"; }
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
            _rsTargetConnector.Connect(connectionProperties);

        }

        #region Get Object Definition Tests


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
            MethodResult methodResult = _rsTargetConnector.ExecuteMethod(methodInput);

            //since we are requesting a list of Table definitions we want to ensure that everything 
            //went as planned by check that the properties have been returned
            Assert.AreNotSame(methodResult.Return.Properties.Count, 0);
        }

        /// <summary>
        /// This is a negative test for when a request os made for an object that does not exist
        /// </summary>
        [TestMethod]
        public void GetObjectDefinitionInValidObjectNameTest()
        {

            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetObjectDefinition" };

            //Add an input property for the 'ObjectName' that is being requested
            methodInput.Input.Properties.Add("ObjectName", InvalidPropertyValue);

            //execute the selected method and set the method result
            MethodResult methodResult = _rsTargetConnector.ExecuteMethod(methodInput);

            //since this is not an error that throws an exception success will be 
            //false and the error information will be propery filled out
            Assert.IsFalse(methodResult.Success);
            Assert.IsNotNull(methodResult.ErrorInfo);
        }

        /// <summary>
        /// This is a negative test for when a Property is not created in the MethodInput
        /// </summary>
        [TestMethod]
        public void GetObjectDefinitionInValidPropertyNameTest()
        {
            //create a new instance of the method input class found in ConnectorApi.Actions
            //Assign the MethodInput name to the Name of the method to be executed
            MethodInput methodInput = new MethodInput { Name = "GetObjectDefinition" };

            //Note: the 'ObjectName' property has not been set

            //execute the selected method
            MethodResult result = _rsTargetConnector.ExecuteMethod(methodInput);
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorInfo);
        }
        #endregion

        #region CreateOrUpdateObjectForReplicaiton

        /// <summary>
        /// This is a positive test for creating a new table
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateObjectForReplicationValidTest()
        {
            MethodInput methodInput = new MethodInput { Name = "CreateOrUpdateObjectForReplication" };

            DataEntity tableEntity = new DataEntity { ObjectDefinitionFullName = "Parameters" };

            EntityProperties properties = new EntityProperties { { "Name", "CherryCola" } };

            EntityChildren columns = new EntityChildren();

            List<DataEntity> columnList = new List<DataEntity>();

            DataEntity column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ColaId");
            column.Properties.Add("DataType", typeof(Guid));
            column.Properties.Add("MaximumLength", 50);
            column.Properties.Add("Nullable", true);
            column.Properties.Add("InPrimaryKey", true);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Cost");
            column.Properties.Add("DataType", typeof(decimal));
            column.Properties.Add("MaximumLength", 38);
            column.Properties.Add("Nullable", true);
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Calories");
            column.Properties.Add("DataType", typeof(int));
            column.Properties.Add("MaximumLength", 38);
            column.Properties.Add("Nullable", true);
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Volume");
            column.Properties.Add("DataType", typeof(int));
            column.Properties.Add("MaximumLength", 38);
            column.Properties.Add("Nullable", true);
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            columns.Add("RSPropertyDefinitions", columnList);

            tableEntity.Children = columns;
            tableEntity.Properties = properties;

            methodInput.Input = tableEntity;

            MethodResult result = _rsTargetConnector.ExecuteMethod(methodInput);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(Convert.ToBoolean(result.Return.Properties["SchemaChanged"]));
        }

        /// <summary>
        /// This is a positive test for the results of a table that does not have schema changes
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateObjectForReplicationExistingTableTest()
        {
            MethodInput methodInput = new MethodInput { Name = "CreateOrUpdateObjectForReplication" };

            DataEntity tableEntity = new DataEntity { ObjectDefinitionFullName = "Parameters" };

            EntityProperties properties = new EntityProperties { { "Name", "Products" } };

            EntityChildren columns = new EntityChildren();

            List<DataEntity> columnList = new List<DataEntity>();

            DataEntity column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "RecordId");
            column.Properties.Add("DataType", typeof(Guid));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ProductNumber");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", true);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ProductName");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Type");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Type");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "UoMSchedule");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Cost");
            column.Properties.Add("DataType", typeof(decimal));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "StandardCost");
            column.Properties.Add("DataType", typeof(decimal));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "QuantityInStock");
            column.Properties.Add("DataType", typeof(Int32));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "QuantityOnOrder");
            column.Properties.Add("DataType", typeof(Int32));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Discontinued");
            column.Properties.Add("DataType", typeof(Int16));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "CreatedOn");
            column.Properties.Add("DataType", typeof(DateTime));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "CreatedBy");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ModifiedOn");
            column.Properties.Add("DataType", typeof(DateTime));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ModifiedBy");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            columns.Add("RSPropertyDefinitions", columnList);

            tableEntity.Children = columns;
            tableEntity.Properties = properties;

            methodInput.Input = tableEntity;

            MethodResult result = _rsTargetConnector.ExecuteMethod(methodInput);

            Assert.IsTrue(result.Success);
            Assert.IsFalse(Convert.ToBoolean(result.Return.Properties["SchemaChanged"]));
        }
        /// <summary>
        /// This is a negative test to detect when a column has been added
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateObjectForReplicationColumnAddedTest()
        {
            MethodInput methodInput = new MethodInput { Name = "CreateOrUpdateObjectForReplication" };

            DataEntity tableEntity = new DataEntity { ObjectDefinitionFullName = "Parameters" };

            EntityProperties properties = new EntityProperties { { "Name", "Products" } };

            EntityChildren columns = new EntityChildren();

            List<DataEntity> columnList = new List<DataEntity>();

            DataEntity column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "RecordId");
            column.Properties.Add("DataType", typeof(Guid));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ProductNumber");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", true);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ProductName");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            //this is the new column
            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "NewPrice");
            column.Properties.Add("DataType", typeof(decimal));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Type");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Type");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "UoMSchedule");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Cost");
            column.Properties.Add("DataType", typeof(decimal));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "StandardCost");
            column.Properties.Add("DataType", typeof(decimal));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "QuantityInStock");
            column.Properties.Add("DataType", typeof(Int32));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "QuantityOnOrder");
            column.Properties.Add("DataType", typeof(Int32));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Discontinued");
            column.Properties.Add("DataType", typeof(Int16));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "CreatedOn");
            column.Properties.Add("DataType", typeof(DateTime));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "CreatedBy");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ModifiedOn");
            column.Properties.Add("DataType", typeof(DateTime));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ModifiedBy");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            columns.Add("RSPropertyDefinitions", columnList);

            tableEntity.Children = columns;
            tableEntity.Properties = properties;

            methodInput.Input = tableEntity;

            MethodResult result = _rsTargetConnector.ExecuteMethod(methodInput);

            Assert.IsTrue(result.Success);
        }


        /// <summary>
        /// This is a negative test to detect when a column has been removed
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateObjectForReplicationColumnRemovedTest()
        {
            MethodInput methodInput = new MethodInput { Name = "CreateOrUpdateObjectForReplication" };

            DataEntity tableEntity = new DataEntity { ObjectDefinitionFullName = "Parameters" };

            EntityProperties properties = new EntityProperties { { "Name", "Products" } };

            EntityChildren columns = new EntityChildren();

            List<DataEntity> columnList = new List<DataEntity>();

            DataEntity column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "RecordId");
            column.Properties.Add("DataType", typeof(Guid));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ProductNumber");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", true);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ProductName");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Type");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            //Note Type has been removed

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "UoMSchedule");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Cost");
            column.Properties.Add("DataType", typeof(decimal));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "StandardCost");
            column.Properties.Add("DataType", typeof(decimal));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "QuantityInStock");
            column.Properties.Add("DataType", typeof(Int32));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "QuantityOnOrder");
            column.Properties.Add("DataType", typeof(Int32));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "Discontinued");
            column.Properties.Add("DataType", typeof(Int16));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "CreatedOn");
            column.Properties.Add("DataType", typeof(DateTime));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "CreatedBy");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ModifiedOn");
            column.Properties.Add("DataType", typeof(DateTime));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", "ModifiedBy");
            column.Properties.Add("DataType", typeof(string));
            column.Properties.Add("InPrimaryKey", false);
            columnList.Add(column);

            columns.Add("RSPropertyDefinitions", columnList);

            tableEntity.Children = columns;
            tableEntity.Properties = properties;

            methodInput.Input = tableEntity;

            MethodResult result = _rsTargetConnector.ExecuteMethod(methodInput);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(Convert.ToBoolean(result.Return.Properties["SchemaChanged"]));
        }
        #endregion

        #region GetLastReplicationSyncDate Tests

        /// <summary>
        /// This is a validity test for the retrieval of the last date of syncronization from a specific table
        /// </summary>
        [TestMethod]
        public void GetLastReplicationSyncDateValidTest()
        {
            //create hte method input and set the name of the method to execute
            MethodInput methodInput = new MethodInput { Name = "GetLastReplicationSyncDate" };
            //set the name of the table to to check by adding it to the input properties
            methodInput.Input.Properties.Add("ObjectName", "Addresses");
            //set the name of the column that the Modification Date is stored in
            methodInput.Input.Properties.Add("ModificationDateFullName", "ModifiedOn");
            //execute the method
            MethodResult result = _rsTargetConnector.ExecuteMethod(methodInput);
            //verify a success
            Assert.IsTrue(result.Success);
            //verify the method yielded a result
            Assert.IsNotNull(result.Return);
        }

        /// <summary>
        /// This is a negative test for and invalid date value
        /// </summary>
        [TestMethod]
        public void GetLastReplicationSyncDateInvalidModifiedDateTest()
        {
            //create hte method input and set the name of the method to execute
            MethodInput methodInput = new MethodInput { Name = "GetLastReplicationSyncDate" };
            //set the name of the table to to check by adding it to the input properties
            methodInput.Input.Properties.Add("ObjectName", "Addresses");
            //set the name of the column that the Modification Date is stored in
            //note: the invalid date
            methodInput.Input.Properties.Add("ModificationDateFullName", InvalidPropertyValue);
            //execute the method
            MethodResult result = _rsTargetConnector.ExecuteMethod(methodInput);
            //verify not a success
            Assert.IsFalse(result.Success);
            //verify that the error info is filled out
            Assert.IsNotNull(result.ErrorInfo);
        }

        /// <summary>
        /// This is a negative test for and invalid table name
        /// </summary>
        [TestMethod]
        public void GetLastReplicationSyncDateInvalidTableTest()
        {
            //create the method input and set the name of the method to execute
            MethodInput methodInput = new MethodInput { Name = "GetLastReplicationSyncDate" };
            //set the name of the table to to check by adding it to the input properties
            //note: the invalid table name
            methodInput.Input.Properties.Add("ObjectName", InvalidPropertyValue);
            //set the name of the column that the Modification Date is stored in
            methodInput.Input.Properties.Add("ModificationDateFullName", "ModifiedOn");
            //execute the method
            MethodResult result = _rsTargetConnector.ExecuteMethod(methodInput);
            //verify not a success
            Assert.IsFalse(result.Success);
            //verify that the error info is filled out
            Assert.IsNotNull(result.ErrorInfo);
        }

        /// <summary>
        /// This is a negative test for and invalid ModificationDateFullName property
        /// </summary>
        [TestMethod]
        public void GetLastReplicationSyncDateInvalidModifiedColumnTest()
        {
            //create hte method input and set the name of the method to execute
            MethodInput methodInput = new MethodInput { Name = "GetLastReplicationSyncDate" };
            //set the name of the table to to check by adding it to the input properties
            methodInput.Input.Properties.Add("ObjectName", "Addresses");
            //set the name of the column that the Modification Date is stored in
            //note: the invalid property name
            methodInput.Input.Properties.Add(InvalidPropertyValue, "ModifiedOn");
            //execute the method
            MethodResult result = _rsTargetConnector.ExecuteMethod(methodInput);
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorInfo);
        }

        /// <summary>
        /// This is a negative test for a missing ModificationDateFullName property
        /// </summary>
        [TestMethod]
        public void GetLastReplicationSyncDateInvalidMissingModifiedColumnTest()
        {
            //create hte method input and set the name of the method to execute
            MethodInput methodInput = new MethodInput { Name = "GetLastReplicationSyncDate" };
            //set the name of the table to to check by adding it to the input properties
            methodInput.Input.Properties.Add("ObjectName", "Addresses");
            //set the name of the column that the Modification Date is stored in

            //note: the missing ModificationDateFullName property

            //execute the method
            MethodResult result = _rsTargetConnector.ExecuteMethod(methodInput);
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorInfo);
        }

        #endregion
    }
}
