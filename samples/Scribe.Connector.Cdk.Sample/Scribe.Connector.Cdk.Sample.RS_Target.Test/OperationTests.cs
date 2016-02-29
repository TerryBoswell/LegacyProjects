// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationTests.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Cryptography;
using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.Query;

namespace Scribe.Connector.Cdk.Sample.RS_Target.Test
{
    [TestClass]
    public class OperationTests
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

        #region Delete Tests
        /// <summary>
        /// This is a positive test for the delete operation
        /// </summary>
        [TestMethod]
        public void DeleteValidTest()
        {
            //create a new data entity
            DataEntity entity = new DataEntity();
            //create a new operation input with a new entity array for the input property
            OperationInput operationInput = new OperationInput { Input = new DataEntity[1] };
            //set the first item in the input property
            operationInput.Input[0] = entity;
            operationInput.Input[0].ObjectDefinitionFullName = "ScribeChangeHistory";
            //set the name of the operation
            operationInput.Name = "Delete";

            //create the comparison experssion for selecting the records to delete
            ComparisonExpression comparisonExpression = new ComparisonExpression();
            comparisonExpression.ExpressionType = ExpressionType.Comparison;
            comparisonExpression.Operator = ComparisonOperator.Less;
            comparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Constant, Value = "ModifiedOn" };
            comparisonExpression.RightValue = new ComparisonValue { ValueType = ComparisonValueType.Variable, Value = DateTime.UtcNow };

            operationInput.LookupCondition[0] = comparisonExpression;
            //execute the operation from the connector
            OperationResult operationResult = _rsTargetConnector.ExecuteOperation(operationInput);
            //validate that the operation was a success
            Assert.IsTrue(operationResult.Success[0]);
        }

        /// <summary>
        /// This is a negative test for the delete operation when no rows have been found
        /// </summary>
        [TestMethod]
        public void DeleteNoRowsFoundInValidTest()
        {
            //create a new data entity
            DataEntity entity = new DataEntity();
            //create a new operation input with a new entity array for the input property
            OperationInput operationInput = new OperationInput { Input = new DataEntity[1] };
            //set the first item in the input property
            operationInput.Input[0] = entity;
            operationInput.Input[0].ObjectDefinitionFullName = "ScribeChangeHistory";
            //set the name of the operation
            operationInput.Name = "Delete";

            //create the comparison experssion for selecting the records to delete
            ComparisonExpression comparisonExpression = new ComparisonExpression();
            comparisonExpression.ExpressionType = ExpressionType.Comparison;
            comparisonExpression.Operator = ComparisonOperator.Less;
            comparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Constant, Value = "ModifiedOn" };
            comparisonExpression.RightValue = new ComparisonValue { ValueType = ComparisonValueType.Variable, Value = DateTime.MinValue };

            operationInput.LookupCondition[0] = comparisonExpression;
            //execute the operation from the connector
            OperationResult operationResult = _rsTargetConnector.ExecuteOperation(operationInput);
            //validate that the operation was not a success
            Assert.IsFalse(operationResult.Success[0]);
            //validate that no rows have been found
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }
        #endregion

        #region Create Tests
        /// <summary>
        /// This is a positive test for inserting a new row of data
        /// </summary>
        [TestMethod]
        public void InsertRowValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput {Name = "create"};

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create a DataEntity for the row 
            table.ObjectDefinitionFullName = "Products";
            columnData.Add("RecordId", Guid.NewGuid().ToString());
            columnData.Add("ProductNumber", DateTime.Now.GetHashCode());
            columnData.Add("ProductName", "Screwdriver");
            columnData.Add("Type", "FinishGood");
            columnData.Add("UoMSchedule", "65");
            columnData.Add("ListPrice", "65");
            columnData.Add("Cost", "65");
            columnData.Add("StandardCost", "65");
            columnData.Add("QuantityInStock", "65");
            columnData.Add("QuantityOnOrder", "65");
            columnData.Add("Discontinued", "0");
            columnData.Add("CreatedOn", DateTime.Now);
            columnData.Add("ModifiedOn", DateTime.Now);

            //add the row data to the input
            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();
            //execute the selected operation

            //execute the selected operation
            OperationResult operationResult = _rsTargetConnector.ExecuteOperation(operationInput);
            //verify the result is a success
            Assert.IsTrue(operationResult.Success[0]);
            //verify that a row was added
            Assert.IsTrue(operationResult.ObjectsAffected[0] >= 1);
        }
        
        /// <summary>
        /// This is a positive test for inseting a special character into a database field
        /// </summary>
        [TestMethod]
        public void InsertRowSingleQuoteValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "create" };

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create a DataEntity for the row 
            table.ObjectDefinitionFullName = "Products";
            columnData.Add("RecordId", Guid.NewGuid().ToString());
            columnData.Add("ProductNumber", DateTime.Now.GetHashCode());
            columnData.Add("ProductName", "Screwdriver");
            columnData.Add("Type", "O'Neil");
            columnData.Add("UoMSchedule", "65");
            columnData.Add("ListPrice", "65");
            columnData.Add("Cost", "65");
            columnData.Add("StandardCost", "65");
            columnData.Add("QuantityInStock", "65");
            columnData.Add("QuantityOnOrder", "65");
            columnData.Add("Discontinued", "0");
            columnData.Add("CreatedOn", DateTime.Now);
            columnData.Add("ModifiedOn", DateTime.Now);

            //add the row data to the input
            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();
            //execute the selected operation

            //execute the selected operation
            OperationResult operationResult = _rsTargetConnector.ExecuteOperation(operationInput);
            //verify the result is a success
            Assert.IsTrue(operationResult.Success[0]);
            //verify that a row was added
            Assert.IsTrue(operationResult.ObjectsAffected[0] >= 1);
        }
        
        /// <summary>
        /// This is a negative test to check for the appropriate error code 
        /// 2601 is returned in the error info when an attempt to insert 
        /// a row that already exists occurs 
        /// NOTE: The record ID must already exist in the table Products for this test
        /// to be relevent
        /// </summary>
        [TestMethod]
        public void InsertExistingRowInvalidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "create" };

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create a DataEntity for the row 
            table.ObjectDefinitionFullName = "Products";
            columnData.Add("RecordId", "9D595A0B-1C4F-43EF-8C03-B01EC343EF99");
            columnData.Add("ProductNumber", "XXXXXX");
            columnData.Add("ProductName", "Technical Consulting");
            columnData.Add("Type", "Service");
            columnData.Add("UoMSchedule", "Consulting");
            columnData.Add("ListPrice", "150.00");
            columnData.Add("Cost", "0.00");
            columnData.Add("StandardCost", "0.00");
            columnData.Add("QuantityInStock", "0");
            columnData.Add("QuantityOnOrder", "0");
            columnData.Add("Discontinued", "0");
            columnData.Add("CreatedOn", DateTime.Now);
            columnData.Add("ModifiedOn", DateTime.Now);

            //add the row data to the input
            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();
            //execute the selected operation

            //execute the selected operation
            OperationResult operationResult = _rsTargetConnector.ExecuteOperation(operationInput);
            //verify the result is not a success
            Assert.IsFalse(operationResult.Success[0]);
            //verify that a row was added
            Assert.AreEqual(ErrorNumber.DuplicateUniqueKey, operationResult.ErrorInfo[0].Number);
        }

        /// <summary>
        /// This is a negative test to verify that a database error will be 
        /// returned correctly in the error info portion of the operation result
        /// </summary>
        [TestMethod]
        public void InsertUnknownTableInValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput {Name = "create"};

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create a DataEntity for the row
            table.ObjectDefinitionFullName = InvalidPropertyValue;
            columnData.Add("RecordId", Guid.NewGuid().ToString());
            columnData.Add("ProductNumber", "134234g");
            columnData.Add("ProductName", "Screwdriver");
            columnData.Add("Type", "FinishGood");
            columnData.Add("UoMSchedule", "65");
            columnData.Add("ListPrice", "65");
            columnData.Add("Cost", "65");
            columnData.Add("StandardCost", "65");
            columnData.Add("QuantityInStock", "65");
            columnData.Add("QuantityOnOrder", "65");
            columnData.Add("Discontinued", "0");
            columnData.Add("CreatedOn", DateTime.Now);
            columnData.Add("ModifiedOn", DateTime.Now);

            //add the row data to the input
            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();
            //execute the selected operation
            OperationResult operationResult = _rsTargetConnector.ExecuteOperation(operationInput);
            //verify the result is not a success
            Assert.IsFalse(operationResult.Success[0]);
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }
        #endregion

        #region Update Operation Tests
        /// <summary>
        /// This is a positive test to validate updating data
        /// </summary>
        [TestMethod]
        public void UpdateReplicationTest()
        {
            OperationInput operationInput = new OperationInput();

            List<DataEntity> input = new List<DataEntity>();
            DataEntity table = new DataEntity();
            EntityProperties columnData = new EntityProperties();


            operationInput.Name = "update";
            operationInput.AllowMultipleObject = false;

            //create the comparison experssion for selecting the records to update
            operationInput.LookupCondition = new Expression[]
                                        {
                                            new ComparisonExpression(ComparisonOperator.Equal,
                                                                     new ComparisonValue(ComparisonValueType.Constant, "State"),
                                                                     new ComparisonValue(ComparisonValueType.Constant, "MA"),
                                                                     null)
                                        };

            //add the columns to change
            table.ObjectDefinitionFullName = "Addresses";
            columnData.Add("Fax", "NA");
            columnData.Add("ModifiedOn", DateTime.Now);

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            //execute the operation
            OperationResult operationResult = _rsTargetConnector.ExecuteOperation(operationInput);

            Assert.IsTrue(operationResult.Success[0]);
            Assert.IsTrue(operationResult.ObjectsAffected[0] >=1);
        }

        [TestMethod]
        public void UpdateReplicationNoRowsTest()
        {
            //create a new method input and use the appropriate operation name
            var operationInput = new OperationInput {Name = "update", AllowMultipleObject = false};

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create the comparison experssion for selecting the records to update
            operationInput.LookupCondition = new Expression[]
                                        {
                                            new ComparisonExpression(ComparisonOperator.Equal,
                                                                     new ComparisonValue(ComparisonValueType.Constant, "State"),
                                                                     new ComparisonValue(ComparisonValueType.Constant, "DC"),
                                                                     null)
                                        };
            //add the columns to change
            table.ObjectDefinitionFullName = "Addresses";
            columnData.Add("Country", "USA");
            columnData.Add("ModifiedOn", DateTime.Now);

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            //execute the operation
            var operationResult = _rsTargetConnector.ExecuteOperation(operationInput);

            Assert.IsFalse(operationResult.Success[0]);
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }

        [TestMethod]
        public void UpdateReplicationInvalidDateTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "update", AllowMultipleObject = false };

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create the comparison experssion for selecting the records to update
            operationInput.LookupCondition = new Expression[]
                                        {
                                            new ComparisonExpression(ComparisonOperator.Equal,
                                                                     new ComparisonValue(ComparisonValueType.Constant, "Type"),
                                                                     new ComparisonValue(ComparisonValueType.Constant, "Order"),
                                                                     null)
                                        };
            //add the columns to change
            table.ObjectDefinitionFullName = "SalesOrders";
            columnData.Add("OrderDate", InvalidPropertyValue);
            columnData.Add("ModifiedOn", DateTime.Now);

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            var operationResult = _rsTargetConnector.ExecuteOperation(operationInput);

            Assert.IsFalse(operationResult.Success[0]);
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }

        [TestMethod]
        public void UpdateReplicationInvalidIntTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "update", AllowMultipleObject = false };
            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create the comparison experssion for selecting the records to update
            operationInput.LookupCondition = new Expression[]
                                        {
                                            new ComparisonExpression(ComparisonOperator.Equal,
                                                                     new ComparisonValue(ComparisonValueType.Constant, "Region"),
                                                                     new ComparisonValue(ComparisonValueType.Constant, "North"),
                                                                     null)
                                        };
            //add the columns to change
            table.ObjectDefinitionFullName = "Customers";
            columnData.Add("CreditOnHold", "5328475903427853943453245324532453425345324523453453453425345324523452342345");
            columnData.Add("ModifiedOn", DateTime.Now);

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            var operationResult = _rsTargetConnector.ExecuteOperation(operationInput);

            Assert.IsFalse(operationResult.Success[0]);
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }


        [TestMethod]
        public void UpdateReplicationNullValueValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "update", AllowMultipleObject = false };

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create the comparison experssion for selecting the records to update
            operationInput.LookupCondition = new Expression[]
            {
                new ComparisonExpression(
                    ComparisonOperator.IsNull,new ComparisonValue(ComparisonValueType.Constant, "Country"), null, null)
            };

            //add the columns to change
            table.ObjectDefinitionFullName = "Addresses";
            columnData.Add("Country", "USA");
            columnData.Add("ModifiedOn", DateTime.Now);

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            var operationResult = _rsTargetConnector.ExecuteOperation(operationInput);

            Assert.IsTrue(operationResult.Success[0]);
            Assert.IsTrue(operationResult.ObjectsAffected[0] >= 1);
        }
#endregion

        #region Unknown Operation test
        /// <summary>
        /// This is a negative test to verify the results of an opertion that does not exist
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidExecuteOperationException))]
        public void UnknownOperationInvalidTest()
        {
            //create a new operation input with an invalid name
            OperationInput operationInput = new OperationInput {Name = "insert"};
            //Execute the operation in the connector
            _rsTargetConnector.ExecuteOperation(operationInput);
        }
        #endregion
    }
}
