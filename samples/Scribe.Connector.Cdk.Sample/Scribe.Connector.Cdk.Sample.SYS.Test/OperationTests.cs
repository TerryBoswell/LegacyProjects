// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationTests.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2012 Scribe Software Corp. All rights reserved.
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

namespace Scribe.Connector.Cdk.Sample.SYS.Test
{
    [TestClass]
    public class OperationTests
    {
        private static readonly IConnector _sysConnector = new SYSConnector();
        private const string InvalidPropertyValue = "xxxxxx";

        /// <summary>
        /// Decryption key for securly passing sensitive data to the connector
        /// Note: this must be that same one both sides
        /// </summary>
        private static string CryptoKey
        {
            get { return "4DCE8727-E3B8-4BC8-92B5-23CBD343663B"; }
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
            _sysConnector.Connect(connectionProperties);

        }

        #region Create Tests
        /// <summary>
        /// This is a positive test for inserting a new row of data
        /// </summary>
        [TestMethod]
        public void InsertRowValidTest()
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
            columnData.Add("Type", "FinishGood");
            columnData.Add("UoMSchedule", null);
            columnData.Add("ListPrice", "65");
            columnData.Add("Cost", "65");
            columnData.Add("StandardCost", "65");
            columnData.Add("QuantityInStock", "65");
            columnData.Add("QuantityOnOrder", "65");
            columnData.Add("Discontinued", "0");

            //add the row data to the input
            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            //execute the selected operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
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
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
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
            var table = new DataEntity("Customers");

            var columnData = new EntityProperties();

            //create a DataEntity for the row 
            table.ObjectDefinitionFullName = "Customers";
            columnData.Add("CustomerNumber", "ABERDEEN0001");
            columnData.Add("CompanyName", "Aberdeen Inc.");
            columnData.Add("Active", "1");

            //add the row data to the input
            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            //execute the selected operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //verify the result is not a success
            Assert.IsFalse(operationResult.Success[0]);
            //verify that a row was added
            Assert.AreEqual(ErrorNumber.DuplicateUniqueKey, operationResult.ErrorInfo[0].Number);
        }

        /// <summary>
        /// This is a positive test for inserting multiple rows of data
        /// Note: The SupportsBulk flag must be set on the ActionDefinition that corresponds to the 'Create' operation
        /// ActionDefinitions are defined through the RetrieveActionDefinitions method in the MetadataProvider.
        /// </summary>
        [TestMethod]
        public void InsertBulkRowsValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "Create" };

            var input = new List<DataEntity>();
            var screwdriver = new DataEntity("Products");
            var handSaw = new DataEntity("Products");
            var drill = new DataEntity("Products");

            //add the row data to the input
            screwdriver.Properties = new EntityProperties
                                 {
                                     //{"ProductNumber", DateTime.Now.GetHashCode()},
                                     {"ProductNumber", "ZD250"},
                                     {"ProductName", "Screwdriver"},
                                     {"Type", "FinishGood"},
                                     {"UoMSchedule", null},
                                     {"ListPrice", "2"},
                                     {"Cost", "1"},
                                     {"StandardCost", "1"},
                                     {"QuantityInStock", "25"},
                                     {"QuantityOnOrder", "10"},
                                     {"Discontinued", "0"}
                                 };

            //Note: this row should fail because the product number already exists
            handSaw.Properties = new EntityProperties
                                 {
                                     {"ProductNumber", "ZD250"},
                                     {"ProductName", "Hand saw"},
                                     {"Type", "FinishGood"},
                                     {"UoMSchedule", null},
                                     {"ListPrice", "15"},
                                     {"Cost", "5"},
                                     {"StandardCost", "7"},
                                     {"QuantityInStock", "20"},
                                     {"QuantityOnOrder", "15"},
                                     {"Discontinued", "0"}
                                 };

            drill.Properties = new EntityProperties
                                 {
                                     {"ProductNumber", DateTime.UtcNow.GetHashCode()},
                                     {"ProductName", "Drill"},
                                     {"Type", "FinishGood"},
                                     {"UoMSchedule", null},
                                     {"ListPrice", "99"},
                                     {"Cost", "65"},
                                     {"StandardCost", "65"},
                                     {"QuantityInStock", "12"},
                                     {"QuantityOnOrder", "2"},
                                     {"Discontinued", "0"}
                                 };


            //add all records to the input list
            input.Add(screwdriver);
            input.Add(handSaw);
            input.Add(drill);

            operationInput.Input = input.ToArray();

            //execute the selected operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);

            //verify the result is not a success
            Assert.IsTrue(operationResult.Success[0]);
            //validate that only 1 record was processed
            Assert.AreEqual(1, operationResult.ObjectsAffected[0]);

            //verify that the second result was not a success
            Assert.IsFalse(operationResult.Success[1]);
            //validate that no rows were processed
            Assert.AreEqual(0, operationResult.ObjectsAffected[1]);

            //verify that the final insert was a success
            Assert.IsTrue(operationResult.Success[2]);
            //validate that only 1 record was processed
            Assert.AreEqual(1, operationResult.ObjectsAffected[2]);
        }

        /// <summary>
        /// This is a negative test to verify that a database error will be 
        /// returned correctly in the error info portion of the operation result
        /// </summary>
        [TestMethod]
        public void InsertUnknownTableInValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "create" };

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
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //verify the result is not a success
            Assert.IsFalse(operationResult.Success[0]);
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }

        #endregion

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
            operationInput.Input[0].ObjectDefinitionFullName = "Picklists";
            //set the name of the operation
            operationInput.Name = "Delete";

            //create the comparison experssion for selecting the records to delete
            ComparisonExpression comparisonExpression = new ComparisonExpression();
            comparisonExpression.ExpressionType = ExpressionType.Comparison;
            comparisonExpression.Operator = ComparisonOperator.Equal;
            comparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Property, Value = "Code" };
            comparisonExpression.RightValue = new ComparisonValue { ValueType = ComparisonValueType.Constant, Value = "NH" };

            operationInput.LookupCondition[0] = comparisonExpression;
            //execute the operation from the connector
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //validate that the operation was a success
            Assert.IsTrue(operationResult.Success[0]);
            //Validate that only one row of data has been deleted
            //NOTE: this will only work with a clean ScribeSampleRSSource database
            Assert.AreEqual(1, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// This is a negative test for the delete operation that attempts to delete multiple rows 
        /// but AllowMultipleObjects is set to false in the OperationInput
        /// </summary>
        [TestMethod]
        public void DeleteTooManyRowsInvalidTest()
        {
            //create a new data entity
            DataEntity entity = new DataEntity();
            //create a new operation input with a new entity array for the input property
            OperationInput operationInput = new OperationInput { Input = new DataEntity[1] };
            //***   this does not allow multiple rows to be processed with one query
            //      Note: this is the Default value
            operationInput.AllowMultipleObject = false;
            //set the first item in the input property
            operationInput.Input[0] = entity;
            operationInput.Input[0].ObjectDefinitionFullName = "PickLists";
            //set the name of the operation
            operationInput.Name = "Delete";

            //create the comparison experssion for selecting the records to delete
            ComparisonExpression comparisonExpression = new ComparisonExpression();
            comparisonExpression.ExpressionType = ExpressionType.Comparison;
            comparisonExpression.Operator = ComparisonOperator.Equal;
            comparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Property, Value = "Description" };
            comparisonExpression.RightValue = new ComparisonValue { ValueType = ComparisonValueType.Constant, Value = null };

            operationInput.LookupCondition[0] = comparisonExpression;
            //execute the operation from the connector
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //validate that the operation was a success
            Assert.IsFalse(operationResult.Success[0]);
        }

        /// <summary>
        /// This is a positive test for the delete operation that attempts to delete multiple rows 
        /// AllowMultipleObjects is set to true in the OperationInput
        /// </summary>
        [TestMethod]
        public void DeleteManyRowsWithAndComparisonValidTest()
        {
            //create a new data entity
            DataEntity entity = new DataEntity();
            //create a new operation input with a new entity array for the input property
            OperationInput operationInput = new OperationInput { Input = new DataEntity[1] };
            //*** this allows multiple rows to be processed with one query
            operationInput.AllowMultipleObject = true;
            //set the first item in the input property
            operationInput.Input[0] = entity;
            operationInput.Input[0].ObjectDefinitionFullName = "Picklists";
            //set the name of the operation
            operationInput.Name = "Delete";

            //create the right comparison experssion for selecting the records to delete
            ComparisonExpression leftComparisonExpression = new ComparisonExpression();
            leftComparisonExpression.ExpressionType = ExpressionType.Comparison;
            leftComparisonExpression.Operator = ComparisonOperator.Equal;
            leftComparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Property, Value = "PickListName" };
            leftComparisonExpression.RightValue = new ComparisonValue { ValueType = ComparisonValueType.Constant, Value = "ShippingMethods" };

            //create the left comparison experssion for selecting the records to delete
            ComparisonExpression rightComparisonExpression = new ComparisonExpression();
            rightComparisonExpression.ExpressionType = ExpressionType.Comparison;
            rightComparisonExpression.Operator = ComparisonOperator.Like;
            rightComparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Property, Value = "Code" };
            rightComparisonExpression.RightValue = new ComparisonValue { ValueType = ComparisonValueType.Constant, Value = "%FedEx%" };

            //create a logical expression which will combine the left and right comparison expressions using an AND operator
            LogicalExpression logicalExpression = new LogicalExpression();
            logicalExpression.ExpressionType = ExpressionType.Logical;
            logicalExpression.LeftExpression = leftComparisonExpression;
            logicalExpression.RightExpression = rightComparisonExpression;
            logicalExpression.Operator = LogicalOperator.And;

            //set the logical expression as the parent of the right and left comparison expressions
            leftComparisonExpression.ParentExpression = logicalExpression;
            rightComparisonExpression.ParentExpression = logicalExpression;

            operationInput.LookupCondition[0] = logicalExpression;
            //execute the operation from the connector
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //validate that the operation was a success
            Assert.IsTrue(operationResult.Success[0]);
            //Validate the amount of rows that have been delete
            //NOTE: this will only work with a clean ScribeSampleRSSource database
            Assert.AreEqual(2, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// This is a positive test for the delete operation that attempts to delete multiple rows 
        /// </summary>
        [TestMethod]
        public void DeleteManyRowsValidTest()
        {
            //create a new data entity
            DataEntity entity = new DataEntity();
            //create a new operation input with a new entity array for the input property
            OperationInput operationInput = new OperationInput { Input = new DataEntity[1] };
            //*** this allows multiple rows to be processed with one query
            operationInput.AllowMultipleObject = true;
            //set the first item in the input property
            operationInput.Input[0] = entity;
            operationInput.Input[0].ObjectDefinitionFullName = "Picklists";
            //set the name of the operation
            operationInput.Name = "Delete";

            //create the comparison experssion for selecting the records to delete
            ComparisonExpression comparisonExpression = new ComparisonExpression();
            comparisonExpression.ExpressionType = ExpressionType.Comparison;
            comparisonExpression.Operator = ComparisonOperator.Like;
            comparisonExpression.LeftValue = new ComparisonValue
                                                 {
                                                     ValueType = ComparisonValueType.Property,
                                                     Value = "Description"
                                                 };
            comparisonExpression.RightValue = new ComparisonValue { ValueType = ComparisonValueType.Constant, Value = "%east%" };


            operationInput.LookupCondition[0] = comparisonExpression;
            //execute the operation from the connector
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //validate that the operation was a success
            Assert.IsTrue(operationResult.Success[0]);
            //Validate the amount of rows that have been delete
            //NOTE: this will only work with a clean ScribeSampleRSSource database
            Assert.AreEqual(2, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// THis is a positive test for deleting multiple rows using bulk support
        /// Note: The SupportsBulk flag must be set on the ActionDefinition that corresponds to the 'Delete' operation
        /// ActionDefinitions are defined through the RetrieveActionDefinitions method in the MetadataProvider.
        /// NOTE: this will only work with a clean ScribeSampleRSSource database
        /// </summary>
        [TestMethod]
        public void DeleteBulkRowsValidTest()
        {
            //create a new data entities
            DataEntity micronesia = new DataEntity("Picklists");
            DataEntity samoa = new DataEntity("Picklists");
            DataEntity doesNotExist = new DataEntity("Picklists");
            DataEntity alberta = new DataEntity("Picklists");
            
            //add the data entities to the input list
            var input = new List<DataEntity> {micronesia, samoa, doesNotExist, alberta};

            //create a new operation input and set the name of the operation
            OperationInput operationInput = new OperationInput("Delete");
            //*** this allows multiple rows to be processed with one query
            operationInput.AllowMultipleObject = false;
            //set the input property
            operationInput.Input = input.ToArray();

            //create the comparison experssion for selecting the micronesia data entitiy
            ComparisonExpression micronesiaExpression = new ComparisonExpression();
            micronesiaExpression.ExpressionType = ExpressionType.Comparison;
            micronesiaExpression.Operator = ComparisonOperator.Equal;
            micronesiaExpression.LeftValue = new ComparisonValue
            { ValueType = ComparisonValueType.Property, Value = "Description" };
            micronesiaExpression.RightValue = new ComparisonValue 
            { ValueType = ComparisonValueType.Constant, Value = "Federated States of Micronesia" };

            //create the comparison experssion for selecting the samoa data entitiy
            ComparisonExpression samoaExpression = new ComparisonExpression();
            samoaExpression.ExpressionType = ExpressionType.Comparison;
            samoaExpression.Operator = ComparisonOperator.Equal;
            samoaExpression.LeftValue = new ComparisonValue 
            { ValueType = ComparisonValueType.Property, Value = "Description" };
            samoaExpression.RightValue = new ComparisonValue 
            { ValueType = ComparisonValueType.Constant, Value = "American Samoa" };

            //create the comparison experssion for selecting the doesNotExist data entitiy
            ComparisonExpression doesNotExistExpression = new ComparisonExpression();
            doesNotExistExpression.ExpressionType = ExpressionType.Comparison;
            doesNotExistExpression.Operator = ComparisonOperator.Equal;
            doesNotExistExpression.LeftValue = new ComparisonValue 
            { ValueType = ComparisonValueType.Property, Value = "Description" };
            //since this value does not exist in the table it will result in an error
            doesNotExistExpression.RightValue = new ComparisonValue 
            { ValueType = ComparisonValueType.Constant, Value = "Does Not Exist" };

            //create the comparison experssion for selecting the alberta data entitiy
            ComparisonExpression albertaExpression = new ComparisonExpression();
            albertaExpression.ExpressionType = ExpressionType.Comparison;
            albertaExpression.Operator = ComparisonOperator.Equal;
            albertaExpression.LeftValue = new ComparisonValue 
            { ValueType = ComparisonValueType.Property, Value = "Description" };
            albertaExpression.RightValue = new ComparisonValue 
            { ValueType = ComparisonValueType.Constant, Value = "Alberta" };

            // Create a list to hold the expressions.
            // Notice the expressions will be in the same order as the data entities.
            // The number of expressions will always be the same as the number of data entities.
            var expressions = new List<Expression>();
            expressions.Add(micronesiaExpression);
            expressions.Add(samoaExpression);
            expressions.Add(doesNotExistExpression);
            expressions.Add(albertaExpression);
            operationInput.LookupCondition = expressions.ToArray();

            //execute the operation from the connector
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);

            // *** Validate the results of the operation, note the number of results
            // *** must be equal to the number of inputs and must also share the same index.

            //validate the micronesion delete operation
            Assert.IsTrue(operationResult.Success[0]);
            Assert.AreEqual(1, operationResult.ObjectsAffected[0]);

            //validate the samoa delete operation
            Assert.IsTrue(operationResult.Success[1]);
            Assert.AreEqual(1, operationResult.ObjectsAffected[1]);

            //validate the does not exist delete operation
            Assert.IsTrue(operationResult.Success[2]);
            Assert.AreEqual(0, operationResult.ObjectsAffected[2]);
            Assert.IsNotNull(operationResult.ErrorInfo[2].Description);

            //validate the micronesion delete operaiton
            Assert.IsTrue(operationResult.Success[3]);
            Assert.AreEqual(1, operationResult.ObjectsAffected[3]);
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
            operationInput.Input[0].ObjectDefinitionFullName = "Products";
            //set the name of the operation
            operationInput.Name = "Delete";

            //create the comparison experssion for selecting the records to delete
            ComparisonExpression comparisonExpression = new ComparisonExpression();
            comparisonExpression.ExpressionType = ExpressionType.Comparison;
            comparisonExpression.Operator = ComparisonOperator.Greater;
            comparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Property, Value = "Products.ModifiedOn" };
            comparisonExpression.RightValue = new ComparisonValue { ValueType = ComparisonValueType.Constant, Value = DateTime.UtcNow };

            operationInput.LookupCondition[0] = comparisonExpression;
            //execute the operation from the connector
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //validate that the operation was a success
            Assert.IsTrue(operationResult.Success[0]);
            //validate that no rows have been found
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
            //validate the proper error code has been returned
            //Note: this error code can be found in the connector in ErrorCodes.cs
            Assert.AreEqual(17, operationResult.ErrorInfo[0].Number);
        }
        #endregion

        #region Update Tests
        /// <summary>
        /// This is a positive test to validate updating data
        /// </summary>
        [TestMethod]
        public void UpdateValidTest()
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
                                                                     new ComparisonValue(ComparisonValueType.Property, "Addresses.CustomerNumber"),
                                                                     new ComparisonValue(ComparisonValueType.Constant, "LITWAREI0001"),
                                                                     null)
                                        };

            //add the columns to change
            table.ObjectDefinitionFullName = "Addresses";
            columnData.Add("Fax", "NA");

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            //execute the operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);

            //validate that the operation was successfull
            Assert.IsTrue(operationResult.Success[0]);
            //validate that only one records has been updated
            Assert.AreEqual(1, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// This is a validity test for attempting to use a boolean value in place of a field that accepts only a bit value
        /// </summary>
        [TestMethod]
        public void UpdateBooleanTrueValidTest()
        {
            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();
            var operationInput = new OperationInput();
            operationInput.Name = "update";

            operationInput.AllowMultipleObject = false;

            //create a new comparison expression that will only attempt to update one row of data
            operationInput.LookupCondition = new Expression[]
                                        {
                                            new ComparisonExpression(ComparisonOperator.Equal,
                                                                     new ComparisonValue(ComparisonValueType.Property, "ProductNumber"),
                                                                     new ComparisonValue(ComparisonValueType.Constant,"KB412"),
                                                                     null)
                                        };

            table.ObjectDefinitionFullName = "Products";
            //This will only accept a value that has a value of 1, or 0 for TRUE or FALSE
            columnData.Add("Discontinued", "True");

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            var operationResult = _sysConnector.ExecuteOperation(operationInput);
            //validate that the operation was not a success
            Assert.IsFalse(operationResult.Success[0]);
            //validate that no objects have been affected
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// This is a negative test for attempting to use a non-boolean value in place of a field that accepts only a bit value
        /// </summary>
        [TestMethod]
        public void UpdateBooleanIncorrectDataTypeInvalidTest()
        {
            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();
            var operationInput = new OperationInput();
            operationInput.Name = "update";

            operationInput.AllowMultipleObject = false;

            //create a new comparison expression that will only attempt to update one row of data
            operationInput.LookupCondition = new Expression[]
                                        {
                                            new ComparisonExpression(ComparisonOperator.Equal,
                                                                     new ComparisonValue(ComparisonValueType.Property, "ProductNumber"),
                                                                     new ComparisonValue(ComparisonValueType.Constant,"DVD20"),
                                                                     null)
                                        };

            table.ObjectDefinitionFullName = "Products";
            //This will only accept a value that has a value of 1, or 0 for TRUE or FALSE
            columnData.Add("Discontinued", "somestring");

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            var operationResult = _sysConnector.ExecuteOperation(operationInput);

            //validate that the result of the operation was not success
            Assert.IsFalse(operationResult.Success[0]);
            //validate that no objects have been affected
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }

        [TestMethod]
        public void UpdateBooleanValidTest()
        {
            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();
            var operationInput = new OperationInput();
            operationInput.Name = "update";

            operationInput.AllowMultipleObject = false;

            //create a new comparison expression that will only attempt to update one row of data
            operationInput.LookupCondition = new Expression[]
                                        {
                                            new ComparisonExpression(ComparisonOperator.Equal,
                                                                     new ComparisonValue(ComparisonValueType.Property, "ProductNumber"),
                                                                     new ComparisonValue(ComparisonValueType.Constant,"ME256"),
                                                                     null)
                                        };


            table.ObjectDefinitionFullName = "Products";
            //This will only accept a value that has a value of 1, or 0 for TRUE or FALSE
            columnData.Add("Discontinued", 1);

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            var operationResult = _sysConnector.ExecuteOperation(operationInput);
            //validate the the result was a success
            Assert.IsTrue(operationResult.Success[0]);
            //validate that only one row of data was affected
            Assert.AreEqual(1, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// This is a negative test for requesting an update that does not have any rows to update
        /// </summary>
        [TestMethod]
        public void UpdateNoRowsTest()
        {
            //create a new method input and use the appropriate operation name
            var operationInput = new OperationInput { Name = "update", AllowMultipleObject = false };

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create the comparison experssion for selecting the records to update
            operationInput.LookupCondition = new Expression[]
                                        {
                                            new ComparisonExpression(ComparisonOperator.Equal,
                                                                     new ComparisonValue(ComparisonValueType.Property, "State"),
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
            var operationResult = _sysConnector.ExecuteOperation(operationInput);

            Assert.IsTrue(operationResult.Success[0]);
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }

        [TestMethod]
        public void UpdateInvalidDateTest()
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
                                                                     new ComparisonValue(ComparisonValueType.Property, "Type"),
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

            var operationResult = _sysConnector.ExecuteOperation(operationInput);

            Assert.IsFalse(operationResult.Success[0]);
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// This is a negative test to validate that an integer field does not take an nvarchar value
        /// </summary>
        [TestMethod]
        public void UpdateInvalidIntTest()
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
                                                                     new ComparisonValue(ComparisonValueType.Property, "Region"),
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

            var operationResult = _sysConnector.ExecuteOperation(operationInput);
            //validate that the result of the operation was not a success
            Assert.IsFalse(operationResult.Success[0]);
            //validate that no objects have been affected
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }


        /// <summary>
        /// This is a validity test for updating mutliple rows with one operation execution
        /// </summary>
        [TestMethod]
        public void UpdateMultipleRowsValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "update", AllowMultipleObject = true };

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create the comparison experssion for selecting the records to update
            operationInput.LookupCondition = new Expression[]
            {
                new ComparisonExpression(
                    ComparisonOperator.IsNull,new ComparisonValue(ComparisonValueType.Property, "TaxSchedule"), null, null)
            };

            //add the columns to change
            table.ObjectDefinitionFullName = "Addresses";
            columnData.Add("TaxSchedule", "ST-PA");

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            //execute the selected operaiton
            var operationResult = _sysConnector.ExecuteOperation(operationInput);

            //validate that the operation was success
            Assert.IsTrue(operationResult.Success[0]);
            //validate that multiple rows have been updated
            Assert.IsTrue(operationResult.ObjectsAffected[0] >= 1);
        }

        /// <summary>
        /// This is a positive test for using multiple filters to update multiple rows of data with one operation execution
        /// </summary>
        [TestMethod]
        public void UpdateMultipleRowsWithComparisonValidTest()
        {
            //create a new data entity
            DataEntity entity = new DataEntity();
            //create a new operation input with a new entity array for the input property
            OperationInput operationInput = new OperationInput { Input = new DataEntity[1] };
            //*** this allows multiple rows to be processed with one query
            operationInput.AllowMultipleObject = true;

            //set the first item in the input property
            entity.ObjectDefinitionFullName = "Addresses";
            entity.Properties.Add("Country", "USA");
            operationInput.Input[0] = entity;

            //set the name of the operation
            operationInput.Name = "update";

            //create the right comparison experssion for selecting the records to update
            ComparisonExpression leftComparisonExpression = new ComparisonExpression();
            leftComparisonExpression.ExpressionType = ExpressionType.Comparison;
            leftComparisonExpression.Operator = ComparisonOperator.IsNull;
            leftComparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Property, Value = "Phone" };
            leftComparisonExpression.RightValue = null;

            //create the left comparison experssion for selecting the records to update
            ComparisonExpression rightComparisonExpression = new ComparisonExpression();
            rightComparisonExpression.ExpressionType = ExpressionType.Comparison;
            rightComparisonExpression.Operator = ComparisonOperator.IsNull;
            rightComparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Property, Value = "AddressLine2" };
            rightComparisonExpression.RightValue = null;

            //create a logical expression which will combine the left and right comparison expressions using an AND operator
            LogicalExpression logicalExpression = new LogicalExpression();
            logicalExpression.ExpressionType = ExpressionType.Logical;
            logicalExpression.LeftExpression = leftComparisonExpression;
            logicalExpression.RightExpression = rightComparisonExpression;
            logicalExpression.Operator = LogicalOperator.And;

            //set the logical expression as the parent of the right and left comparison expressions
            leftComparisonExpression.ParentExpression = logicalExpression;
            rightComparisonExpression.ParentExpression = logicalExpression;

            operationInput.LookupCondition[0] = logicalExpression;
            //execute the operation from the connector
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //validate that the operation was a success
            Assert.IsTrue(operationResult.Success[0]);
            //Validate the amount of rows that have been updated
            //NOTE: this will only work with a clean ScribeSampleRSSource database
            Assert.AreEqual(10, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// This is a negative test to validate that an update will not execute if 
        /// multiple rows are selected when the OperationInput does not allow it
        /// </summary>
        [TestMethod]
        public void UpdateTooManyRowsInvalidTest()
        {
            OperationInput operationInput = new OperationInput();

            List<DataEntity> input = new List<DataEntity>();
            DataEntity table = new DataEntity();
            EntityProperties columnData = new EntityProperties();

            operationInput.Name = "update";

            //note: altering of multiple rows is not allowed
            operationInput.AllowMultipleObject = false;

            //create the comparison experssion for selecting the records to update
            operationInput.LookupCondition = new Expression[]
                                        {
                                            new ComparisonExpression(ComparisonOperator.IsNull,
                                                                     new ComparisonValue(ComparisonValueType.Constant, "Description"),
                                                                     null,
                                                                     null)
                                        };

            //add the columns to change
            table.ObjectDefinitionFullName = "PickLists";
            columnData.Add("Description", "");

            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            //execute the operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //validate that the update was a success
            Assert.IsTrue(operationResult.Success[0]);

            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// This is a positive test for upserting multiple rows of data
        /// Note: The SupportsBulk flag must be set on the ActionDefinition that corresponds to the 'Update' operation
        /// ActionDefinitions are defined through the RetrieveActionDefinitions method in the MetadataProvider.
        /// </summary>
        [TestMethod]
        public void UpdateBulkRowsValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "Update" };
            
            var inputs = new List<DataEntity>();
            var lookupConditions = new List<Expression>();


            // Each update will consist of one DataEntity object and one LookupCondition object.
            // The reletated objects have the same indexes in their corresponding arrays.
            var retail = new DataEntity("Picklists");
            //add the row data to the input
            retail.Properties = new EntityProperties { { "Description", DateTime.UtcNow } };
            //Generate the comparison expression for retail, this will define the where clause portion of the query.
            var retailLookupCondition = new ComparisonExpression( ComparisonOperator.Equal,
                new ComparisonValue(ComparisonValueType.Property, "Code"),
                new ComparisonValue(ComparisonValueType.Constant, "Retail"), 
                null);

            inputs.Add(retail);
            lookupConditions.Add(retailLookupCondition);

            var priceLists = new DataEntity("Picklists");
            priceLists.Properties = new EntityProperties { { "Code", "Wholesale" } };
            // Note: This record will return success but with not rows processed
            //       because 'PriceList' does not exist in the [Code] column
            var priceListsLookupCondition = new ComparisonExpression(
                ComparisonOperator.Equal,
                new ComparisonValue(ComparisonValueType.Property, "Code"),
                new ComparisonValue(ComparisonValueType.Constant, "PriceList"),
                null);

            inputs.Add(priceLists);
            lookupConditions.Add(priceListsLookupCondition);

            var finishGood = new DataEntity("Picklists");
            // Note: This record will fail because null is not a valid value for PickListName
            finishGood.Properties = new EntityProperties {{"PickListName", null}};
            var finishGoodLookupCondition = new ComparisonExpression(
                ComparisonOperator.Equal,
                new ComparisonValue(ComparisonValueType.Property, "Code"),
                new ComparisonValue(ComparisonValueType.Constant, "FinishGood"),
                null);

            inputs.Add(finishGood);
            lookupConditions.Add(finishGoodLookupCondition);

            var invoiceType = new DataEntity("Picklists");
            invoiceType.Properties = new EntityProperties
                                 { {"Description", DateTime.UtcNow } };
            var invoiceLookupCondition = new ComparisonExpression(
                ComparisonOperator.Equal,
                new ComparisonValue(ComparisonValueType.Property, "Code"),
                new ComparisonValue(ComparisonValueType.Constant, "Invoice"),
                null);

            // Add the data entity and lookup condition.
            // Note: This is the same order the results MUST be returned.
            inputs.Add(invoiceType);
            lookupConditions.Add(invoiceLookupCondition);

            // Input and LookupCondition will be received by the connector as an array. 
            operationInput.Input = inputs.ToArray();
            operationInput.LookupCondition = lookupConditions.ToArray();

            //execute the selected operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);

            //verify the result is not a success
            Assert.IsTrue(operationResult.Success[0]);
            //validate that only 1 record was processed
            Assert.AreEqual(1, operationResult.ObjectsAffected[0]);

            //verify the result is not a success
            Assert.IsTrue(operationResult.Success[1]);
            //validate that no records were processed
            Assert.AreEqual(0, operationResult.ObjectsAffected[1]);

            //verify that the second result was not a success
            Assert.IsFalse(operationResult.Success[2]);
            //validate that no rows were processed
            Assert.AreEqual(0, operationResult.ObjectsAffected[2]);

            //verify that the final update was a success
            Assert.IsTrue(operationResult.Success[3]);
            //validate that only 1 record was processed
            Assert.AreEqual(1, operationResult.ObjectsAffected[3]);
        }
        #endregion

        #region Upsert Tests
        /// <summary>
        /// This is a positive test for upserting a new row of data
        /// </summary>
        [TestMethod]
        public void UpsertRowValidTest()
        {
            //Create a new method input and use the appropriate operation name.
            OperationInput operationInput = new OperationInput { Name = "upsert" };

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //Create a DataEntity for the record. 
            table.ObjectDefinitionFullName = "Products";
            //Guarentees a new record every time.
            columnData.Add("ProductNumber", DateTime.UtcNow.ToFileTime());
            columnData.Add("ProductName", "Screwdriver");
            columnData.Add("Type", "FinishGood");
            columnData.Add("UoMSchedule", null);
            columnData.Add("ListPrice", "65");
            columnData.Add("Cost", "65");
            columnData.Add("StandardCost", "65");
            columnData.Add("QuantityInStock", "65");
            columnData.Add("QuantityOnOrder", "65");
            columnData.Add("Discontinued", "0");

            //add the row data to the input
            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            //execute the selected operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //verify the result is a success
            Assert.IsTrue(operationResult.Success[0]);
            //verify that a row was added
            Assert.IsTrue(operationResult.ObjectsAffected[0] >= 1);
        }

        /// <summary>
        /// This is a negative test for upserting a new row of data that does not contain a primary key value
        /// </summary>
        [TestMethod]
        public void UpsertRowNoKeySpecifiedInValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "upsert" };

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create a DataEntity for the row 
            table.ObjectDefinitionFullName = "Products";
            //note: no primary key specified
            //columnData.Add("ProductNumber", GetHashCode());
            columnData.Add("ProductName", "Screwdriver");
            columnData.Add("Type", "FinishGood");
            columnData.Add("UoMSchedule", null);
            columnData.Add("ListPrice", "65");
            columnData.Add("Cost", "65");
            columnData.Add("StandardCost", "65");
            columnData.Add("QuantityInStock", "65");
            columnData.Add("QuantityOnOrder", "65");
            columnData.Add("Discontinued", "0");

            //add the row data to the input
            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            //execute the selected operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //verify the result is a success
            Assert.IsFalse(operationResult.Success[0]);
            //verify that a row was added
            Assert.IsTrue(operationResult.ObjectsAffected[0] >= 0);
        }

        /// <summary>
        /// This is a positive test to update an existing row using upsert
        /// </summary>
        [TestMethod]
        public void UpsertingExistingRowInvalidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "upsert" };

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create a DataEntity for the row 
            table.ObjectDefinitionFullName = "Customers";

            columnData.Add("CustomerNumber", "ABERDEEN0001");
            columnData.Add("CompanyName", "Aberdeen Inc.");
            columnData.Add("Active", "1");
            columnData.Add("Email", "aberdeen@abd.com");

            //add the row data to the input
            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();

            //execute the selected operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //verify the result is a success
            Assert.IsTrue(operationResult.Success[0]);
            //verify that a row was added
            Assert.AreEqual(1, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// This is a negative test to verify that a database error will be 
        /// returned correctly in the error info portion of the operation result
        /// </summary>
        [TestMethod]
        public void UpsertUnknownTableInValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "upsert" };

            var input = new List<DataEntity>();
            var table = new DataEntity();
            var columnData = new EntityProperties();

            //create a DataEntity for the row
            table.ObjectDefinitionFullName = InvalidPropertyValue;
            columnData.Add("ProductNumber", "1234567890");
            columnData.Add("ProductName", "Screwdriver");
            columnData.Add("Type", "FinishGood");
            columnData.Add("UoMSchedule", "65");
            columnData.Add("ListPrice", "65");
            columnData.Add("Cost", "65");
            columnData.Add("StandardCost", "65");
            columnData.Add("QuantityInStock", "65");
            columnData.Add("QuantityOnOrder", "65");
            columnData.Add("Discontinued", "0");

            //add the row data to the input
            table.Properties = columnData;
            input.Add(table);

            operationInput.Input = input.ToArray();
            //execute the selected operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);
            //verify the result is not a success
            Assert.IsFalse(operationResult.Success[0]);
            Assert.AreEqual(0, operationResult.ObjectsAffected[0]);
        }

        /// <summary>
        /// This is a positive test for upserting multiple rows of data
        /// Note: The SupportsBulk flag must be set on the ActionDefinition that corresponds to the 'UpdateInsert' operation
        /// ActionDefinitions are defined through the RetrieveActionDefinitions method in the MetadataProvider.
        /// </summary>
        [TestMethod]
        public void UpsertBulkRowsValidTest()
        {
            //create a new method input and use the appropriate operation name
            OperationInput operationInput = new OperationInput { Name = "Upsert" };

            var input = new List<DataEntity>();
            var screwdriver = new DataEntity("Products");
            var drill = new DataEntity("Products");
            var handSaw = new DataEntity("Products");

            //add the row data to the input
            screwdriver.Properties = new EntityProperties
                                 {
                                     {"ProductNumber", "ZD250"},
                                     {"ProductName", "Screwdriver"},
                                     {"Type", "FinishGood"},
                                     {"UoMSchedule", null},
                                     {"ListPrice", "2"},
                                     {"Cost", "1"},
                                     {"StandardCost", "1"},
                                     {"QuantityInStock", "25"},
                                     {"QuantityOnOrder", "10"},
                                     {"Discontinued", "0"}
                                 };


            //Note: This record should fail because column 'ProductNumber'
            //      does not allow nulls.
            drill.Properties = new EntityProperties
                                 {
                                     {"ProductNumber", null},
                                     {"ProductName", "Drill"},
                                     {"Type", "FinishGood"},
                                     {"UoMSchedule", null},
                                     {"ListPrice", "99"},
                                     {"Cost", "65"},
                                     {"StandardCost", "65"},
                                     {"QuantityInStock", "12"},
                                     {"QuantityOnOrder", "2"},
                                     {"Discontinued", "0"}
                                 };

            //this row should be an update because the product number already exists
            handSaw.Properties = new EntityProperties
                                 {
                                     {"ProductNumber", "ZD250"},
                                     {"ProductName", "Hand saw"},
                                     {"Type", "FinishGood"},
                                     {"UoMSchedule", null},
                                     {"ListPrice", "15"},
                                     {"Cost", "5"},
                                     {"StandardCost", "7"},
                                     {"QuantityInStock", "20"},
                                     {"QuantityOnOrder", "15"},
                                     {"Discontinued", "0"}
                                 };


            // Add all records to the input list
            // Note: this is the same order the results MUST be returned
            input.Add(screwdriver);
            input.Add(drill);
            input.Add(handSaw);


            operationInput.Input = input.ToArray();

            //execute the selected operation
            OperationResult operationResult = _sysConnector.ExecuteOperation(operationInput);

            //verify the result is not a success
            Assert.IsTrue(operationResult.Success[0]);
            //validate that only 1 record was processed
            Assert.AreEqual(1, operationResult.ObjectsAffected[0]);

            //verify that the second result was not a success
            Assert.IsFalse(operationResult.Success[1]);
            //validate that no rows were processed
            Assert.AreEqual(0, operationResult.ObjectsAffected[1]);

            //verify that the final upsert was a success
            Assert.IsTrue(operationResult.Success[2]);
            //validate that only 1 record was processed
            Assert.AreEqual(1, operationResult.ObjectsAffected[2]);
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
            OperationInput operationInput = new OperationInput { Name = "insert" };
            //Execute the operation in the connector
            _sysConnector.ExecuteOperation(operationInput);
        }
        #endregion
    }
}
