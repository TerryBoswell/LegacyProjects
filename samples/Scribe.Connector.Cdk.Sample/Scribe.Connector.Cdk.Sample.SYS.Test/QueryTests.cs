// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryTests.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Cryptography;
using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.Metadata;
using Scribe.Core.ConnectorApi.Query;

namespace Scribe.Connector.Cdk.Sample.SYS.Test
{
    [TestClass]
    public class QueryTests
    {
        #region SYS Connector Pre Test Setup
        /// <summary>
        /// Create a new dictionary to store the connection properties
        /// </summary>
        private Dictionary<string, string> _connectionProperties = new Dictionary<string, string>();
        private static SYSConnector _sysConnector = new SYSConnector();

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
        /// Setup the initial connection properties prior to performing any tests
        /// </summary>
        [TestInitialize]
        public void StartUp()
        {
            //setup the initial parameters for data connection
            _connectionProperties.Add("Provider", "SQLNCLI10");
            _connectionProperties.Add("Server", "localhost");
            _connectionProperties.Add("Database", "ScribeSampleRSSource");
            _connectionProperties.Add("UserName", "sa");
            //encrypt the connection password using the shared key
            string encryptedPassword = Encryptor.Encrypt_AesManaged("sa", CryptoKey);
            _connectionProperties.Add("Password", encryptedPassword);
            _sysConnector = new SYSConnector();
            _sysConnector.Connect(_connectionProperties);

        }
        #endregion

        #region SYS Connector Post Test Cleanup
        [ClassCleanup]
        public static void Cleanup()
        {
            if (_sysConnector.IsConnected)
            {
                _sysConnector.Disconnect();
            }
        }
        #endregion

        #region ExecuteQuery Tests
        /// <summary>
        /// This is a validity test for the ExecuteQuery method
        /// </summary>
        [TestMethod]
        public void ExecuteQueryValidTest()
        {
            //Declare the name of the table for the test
            string tableName = "Addresses";

            //Retrieve the current metadata provoder
            var metaDataProvider = _sysConnector.GetMetadataProvider();

            //Create a new root Query Enitity
            QueryEntity rootEntity = new QueryEntity();

            //Add the table name to the query entity
            rootEntity.Name = tableName;
            rootEntity.ObjectDefinitionFullName = tableName;

            //Retrieve the column information for the selected table
            IObjectDefinition objectDefinition = metaDataProvider.RetrieveObjectDefinition(tableName, true, true);

            //Add each column to the list of properties
            foreach (IPropertyDefinition propertyDefinition in objectDefinition.PropertyDefinitions)
            {
                rootEntity.PropertyList.Add(propertyDefinition.Name);
            }

            //create a new query object
            Query queryObject = new Query();
            //indicate whether or not this is a test query
            queryObject.IsTestQuery = true;
            //set the root entity
            queryObject.RootEntity = rootEntity;
            //execute the query using the method provided from the connector
            var queryEntity = _sysConnector.ExecuteQuery(queryObject);
            //validate that results have been returned
            Assert.IsNotNull(queryEntity);
            Assert.AreNotEqual(0, queryEntity.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryEntity.ElementAt(0).Properties.Count);
        }

        /// <summary>
        /// This is a negative test for using an empty query
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidExecuteQueryException))]
        public void ExecuteQueryEmptyQueryTest()
        {
            //Execute a new query object
            var queryResult = _sysConnector.ExecuteQuery(new Query());

            //Check that the result is not null
            Assert.IsNotNull(queryResult);

            //Check that now results have been yielded
            Assert.AreEqual(0, queryResult.Count());
        }

        #endregion

        #region Basic Query Tests
        [TestMethod]
        public void BasicQueryValidTest()
        {
            string objectName = "Addresses";
            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity();
            rootEntity.Name = objectName;
            rootEntity.ObjectDefinitionFullName = objectName;
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ContactName");
            rootEntity.PropertyList.Add("Phone");
            rootEntity.PropertyList.Add("AddressType");

            //create a new query object
            Query query = new Query();
            //indicate whether or not this is a test query
            query.IsTestQuery = true;
            //set the root entity
            query.RootEntity = rootEntity;

            //Create a new instance of the Connector
            _sysConnector = new SYSConnector();
            //Establish a connection to the data source
            _sysConnector.Connect(_connectionProperties);
            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
        }

        [TestMethod]
        public void BasicQuerySelectStarValidTest()
        {
            string objectName = "Addresses";
            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity();
            rootEntity.Name = objectName;
            rootEntity.ObjectDefinitionFullName = objectName;

            //Note: No property list has been set, the QueryBuilder should add all columns to the query

            //create a new query object
            Query query = new Query();
            //indicate whether or not this is a test query
            query.IsTestQuery = true;
            //set the root entity
            query.RootEntity = rootEntity;

            //Create a new instance of the Connector
            _sysConnector = new SYSConnector();
            //Establish a connection to the data source
            _sysConnector.Connect(_connectionProperties);
            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
        }

        [TestMethod]
        public void BasicQueryWithFieldFilterValidTest()
        {
            string objectName = "Addresses";

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ContactName");
            rootEntity.PropertyList.Add("ContactTitle");
            rootEntity.PropertyList.Add("AddressType");

            //create a new comparison expression object
            //The following query looks like this:
            // SELECT * FROM Addresses WHERE Addresses.ContactTitle = President
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var comparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.Equal,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "Addresses.ContactTitle"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, "President")
            };

            //create a new query object
            Query query = new Query();
            //indicate whether or not this is a test query
            query.IsTestQuery = true;
            //set the root entity
            query.RootEntity = rootEntity;
            //set the constraints for the query
            query.Constraints = comparisionExpression;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
            //validate that the proper values are returned in the DataEntity properties
            Assert.AreEqual("President", queryResults.ElementAt(0).Properties["ContactTitle"].ToString());
        }
        #endregion

        #region Simple Filter Tests
        [TestMethod]
        public void SimpleAndFilterValidTest()
        {
            string objectName = "Addresses";

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ContactName");
            rootEntity.PropertyList.Add("ContactTitle");
            rootEntity.PropertyList.Add("AddressType");

            //create a new query object, and set the root entity
            var query = new Query { IsTestQuery = false, RootEntity = rootEntity };

            //create a new comparison expression object
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var leftComparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.Equal,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "Addresses.AddressType"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, "Main")
            };

            //create another comparison expression to add on the right of the AND clause
            var rightComparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.Like,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "Addresses.ContactTitle"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, "President")
            };

            //create a new logical expression indicating an AND clause in filtering of the query
            LogicalExpression logicalExpression = new LogicalExpression(
                //Sets the opertor for the expression
                LogicalOperator.And,
                //add the expressions
                leftComparisionExpression,
                rightComparisionExpression,
                //since this is the parent expressions there is no parent to indicate here so set it to null
                null);

            //set the contraints in the query
            query.Constraints = logicalExpression;
            //set the expression type for the query constraints
            query.Constraints.ExpressionType = ExpressionType.Logical;

            //set the parent expression for the right and left comparison expressions since they are now part of the logical expression
            leftComparisionExpression.ParentExpression = query.Constraints;
            rightComparisionExpression.ParentExpression = query.Constraints;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
            //validate that the proper values are returned in the DataEntity properties
            Assert.AreEqual("President", queryResults.ElementAt(0).Properties["ContactTitle"].ToString());
            Assert.AreEqual("Main", queryResults.ElementAt(0).Properties["AddressType"].ToString());
        }

        [TestMethod]
        public void SimpleGreaterThanFilterValidTest()
        {
            string objectName = "ProductPriceLists";

            //create a new comparison expression object
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var comparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.Greater,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "ProductPriceLists.UnitPrice"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, 2)
            };

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ProductNumber");
            rootEntity.PropertyList.Add("CreatedOn");
            rootEntity.PropertyList.Add("UnitPrice");

            //create a new query object
            Query query = new Query();
            //indicate whether or not this is a test query
            query.IsTestQuery = true;
            //set the root entity
            query.RootEntity = rootEntity;
            //set the constraints for the query
            query.Constraints = comparisionExpression;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
            //validate that the proper values are returned in the DataEntity properties
            Assert.IsTrue((decimal)queryResults.ElementAt(0).Properties["UnitPrice"] > 2);
        }

        [TestMethod]
        public void SimpleBooleanComparisonFilterValidTest()
        {
            string objectName = "Products";

            //create a new comparison expression object
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var comparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.Equal,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "Products.Discontinued"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, "false")
            };

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ProductNumber");
            rootEntity.PropertyList.Add("CreatedOn");
            rootEntity.PropertyList.Add("Discontinued");

            //create a new query object
            Query query = new Query();
            //indicate whether or not this is a test query
            query.IsTestQuery = true;
            //set the root entity
            query.RootEntity = rootEntity;
            //set the constraints for the query
            query.Constraints = comparisionExpression;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
            //validate that the proper values are returned in the DataEntity properties
            Assert.AreEqual(queryResults.ElementAt(0).Properties["Discontinued"].ToString(), "0");
        }

        [TestMethod]
        public void SimpleGreaterThanOrEqualFilterValidTest()
        {
            string objectName = "ProductPriceLists";

            //create a new comparison expression object
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var comparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.GreaterOrEqual,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "ProductPriceLists.UnitPrice"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, 2)
            };

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ProductNumber");
            rootEntity.PropertyList.Add("CreatedOn");
            rootEntity.PropertyList.Add("UnitPrice");

            //create a new query object
            Query query = new Query();
            //indicate whether or not this is a test query
            query.IsTestQuery = true;
            //set the root entity
            query.RootEntity = rootEntity;
            //set the constraints for the query
            query.Constraints = comparisionExpression;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
            //validate that the proper values are returned in the DataEntity properties
            Assert.IsTrue((decimal)queryResults.ElementAt(0).Properties["UnitPrice"] >= 2);
        }

        [TestMethod]
        public void SimpleLessThanOrEqualFilterValidTest()
        {
            string objectName = "ProductPriceLists";

            //create a new comparison expression object
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var comparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.LessOrEqual,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "ProductPriceLists.UnitPrice"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, 2)
            };

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ProductNumber");
            rootEntity.PropertyList.Add("CreatedOn");
            rootEntity.PropertyList.Add("UnitPrice");

            //create a new query object
            Query query = new Query();
            //indicate whether or not this is a test query
            query.IsTestQuery = true;
            //set the root entity
            query.RootEntity = rootEntity;
            //set the constraints for the query
            query.Constraints = comparisionExpression;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
            //validate that the proper values are returned in the DataEntity properties
            Assert.IsTrue((decimal)queryResults.ElementAt(0).Properties["UnitPrice"] <= 2);
        }

        [TestMethod]
        public void SimpleLikeFilterValidTest()
        {
            string objectName = "Addresses";

            //create a new comparison expression object
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var comparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.Like,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "Addresses.ContactName"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, "Mr.%")
            };

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ContactName");
            rootEntity.PropertyList.Add("Phone");
            rootEntity.PropertyList.Add("LocationName");

            //create a new query object
            Query query = new Query();
            //indicate whether or not this is a test query
            query.IsTestQuery = true;
            //set the root entity
            query.RootEntity = rootEntity;
            //set the constraints for the query
            query.Constraints = comparisionExpression;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
            //validate that the proper values are returned in the DataEntity properties
            Assert.IsTrue(queryResults.ElementAt(0).Properties["ContactName"].ToString().Contains("Mr."));
        }

        [TestMethod]
        public void SimpleNotLikeFilterValidTest()
        {
            string objectName = "Addresses";

            //create a new comparison expression object
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var comparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.NotLike,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "Addresses.ContactName"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, "Mr.%")
            };

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ContactName");
            rootEntity.PropertyList.Add("Phone");
            rootEntity.PropertyList.Add("LocationName");

            //create a new query object
            Query query = new Query();
            //indicate whether or not this is a test query
            query.IsTestQuery = true;
            //set the root entity
            query.RootEntity = rootEntity;
            //set the constraints for the query
            query.Constraints = comparisionExpression;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
            //validate that the proper values are returned in the DataEntity properties
            Assert.IsFalse(queryResults.ElementAt(0).Properties["ContactName"].ToString().Contains("Mr."));
        }

        [TestMethod]
        public void SimpleDateFilterValidTest()
        {
            string objectName = "Addresses";

            var dateValue = DateTime.Now.AddMonths(-2).ToString("yyyy'-'MM'-'dd HH':'mm':'ss K");

            //create a new comparison expression object
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var comparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.Greater,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "Addresses.ModifiedOn"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, dateValue)
            };

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ContactName");
            rootEntity.PropertyList.Add("Phone");
            rootEntity.PropertyList.Add("ModifiedOn");

            //create a new query object
            Query query = new Query();
            //indicate whether or not this is a test query
            query.IsTestQuery = true;
            //set the root entity
            query.RootEntity = rootEntity;
            //set the constraints for the query
            query.Constraints = comparisionExpression;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
            //validate that the proper values are returned in the DataEntity properties
            Assert.IsTrue((DateTime)queryResults.ElementAt(0).Properties["ModifiedOn"] > Convert.ToDateTime(dateValue));
        }
        #endregion

        #region Order By Query Tests
        [TestMethod]
        public void BasicQueryOrderByDescendingValidTest()
        {
            const string objectName = "ProductPriceLists";

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //add columns to the root entity
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ProductNumber");
            rootEntity.PropertyList.Add("UnitPrice");

            //set the sequence direction
            rootEntity.SequenceList.Add(new Sequence("UnitPrice", SequenceDirection.Descending));

            var query = new Query { IsTestQuery = false, RootEntity = rootEntity };

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);

            //Verify the correct result of the first element returned from the query
            Assert.AreEqual("20000", queryResults.First().Properties["UnitPrice"].ToString());
            Assert.AreEqual("CONSULT", queryResults.First().Properties["ProductNumber"].ToString());

            //Verify the correct result of the last element returned from the query
            Assert.AreEqual("0.0000", queryResults.Last().Properties["UnitPrice"].ToString());
            Assert.AreEqual("SW101", queryResults.Last().Properties["ProductNumber"].ToString());
        }

        [TestMethod]
        public void BasicQueryOrderByAscendingValidTest()
        {
            const string objectName = "ProductPriceLists";

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //add columns to the root entity
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ProductNumber");
            rootEntity.PropertyList.Add("UnitPrice");

            //set the sequence direction, note: Ascending is chosen by default
            rootEntity.SequenceList.Add(new Sequence("UnitPrice"));

            var query = new Query { IsTestQuery = false, RootEntity = rootEntity };

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);

            //Verify the correct result of the last element returned from the query
            Assert.AreEqual("0.0000", queryResults.First().Properties["UnitPrice"].ToString());
            Assert.AreEqual("SW101", queryResults.First().Properties["ProductNumber"].ToString());

            //Verify the correct result of the first element returned from the query
            Assert.AreEqual("20000", queryResults.Last().Properties["UnitPrice"].ToString());
            Assert.AreEqual("CONSULT", queryResults.Last().Properties["ProductNumber"].ToString());
        }

        [TestMethod]
        public void BasicQueryOrderByTwoFieldsValidTest()
        {
            const string objectName = "ProductPriceLists";
            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //add columns to be returned in the query
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ProductNumber");
            rootEntity.PropertyList.Add("UnitPrice");
            rootEntity.PropertyList.Add("BaseUoMQuantity");

            //set the inital sequence direction, note: Ascending is chosen by default
            rootEntity.SequenceList.Add(new Sequence("ProductNumber"));
            //set the subsequent sequence direction
            rootEntity.SequenceList.Add(new Sequence("UnitPrice", SequenceDirection.Descending));

            var query = new Query { IsTestQuery = false, RootEntity = rootEntity };

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);

            //Verify the correct result of the first element returned from the query
            Assert.AreEqual("49.95", queryResults.First().Properties["UnitPrice"].ToString());
            Assert.AreEqual("CAT5CBL", queryResults.First().Properties["ProductNumber"].ToString());

            //Verify the correct result of the last element returned from the query
            Assert.AreEqual("79.95", queryResults.Last().Properties["UnitPrice"].ToString());
            Assert.AreEqual("ZD250", queryResults.Last().Properties["ProductNumber"].ToString());
        }

        [TestMethod]
        public void SimpleFilterWithOrderByValidTest()
        {
            string objectName = "ProductPriceLists";

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ProductNumber");
            rootEntity.PropertyList.Add("UnitPrice");
            rootEntity.PropertyList.Add("BaseUoMQuantity");

            //set the sequence direction and field to order by
            rootEntity.SequenceList.Add(new Sequence("UnitPrice", SequenceDirection.Descending));

            //create a new query object, and set the root entity
            var query = new Query { IsTestQuery = false, RootEntity = rootEntity };

            //create a new comparison expression object
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var comparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.Equal,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "ProductPriceLists.ProductNumber"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, "CAT5CBL")
            };

            //set the contraints in the query to the comparison expression
            query.Constraints = comparisionExpression;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);

            //Verify that the highest price is returned in the first element of the results
            Assert.AreEqual("49.95", queryResults.First().Properties["UnitPrice"].ToString());

            //Verify that the lowest price is returned in the last element of the results
            Assert.AreEqual("0.7", queryResults.Last().Properties["UnitPrice"].ToString());
        }

        [TestMethod]
        public void QueryMultipleFilterWithOrderByValidTest()
        {
            string objectName = "ProductPriceLists";

            //Create a basic root query entity, 
            //Set the name property and the object definition full name property
            //Note: 'Name' property is unique and is set by the user
            //      'ObjectDefinitionFullName' property will be the name of the table referenced
            var rootEntity = new QueryEntity { Name = objectName, ObjectDefinitionFullName = objectName };
            //Create a list of properties for the root entity, not these are column names
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("ProductNumber");
            rootEntity.PropertyList.Add("UnitPrice");
            rootEntity.PropertyList.Add("BaseUoMQuantity");

            //set the sequence direction and field to order by
            rootEntity.SequenceList.Add(new Sequence("UnitPrice", SequenceDirection.Descending));

            //create a new comparison expression object
            // consider **** SELECT [QueryEntity.PropertyList] FROM [QueryEntity.ObjectDefinitionFullName]
            //          **** WHERE [ComparisonExpression.LeftValue] [ComparisonExpression.Operator] [ComparisonExpression.RightValue]
            var leftComparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.Equal,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "ProductPriceLists.ProductNumber"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, "CONSULT")
            };

            //create another comparison expression to add on the right of the AND clause
            var rightComparisionExpression = new ComparisonExpression
            {
                ExpressionType = ExpressionType.Comparison,
                Operator = ComparisonOperator.Less,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "ProductPriceLists.BaseUoMQuantity"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, 50)
            };

            //create a new query object, and set the root entity
            var query = new Query { IsTestQuery = false, RootEntity = rootEntity };

            //create a new logical expression indicating an AND clause in filtering of the query
            LogicalExpression logicalExpression = new LogicalExpression(
                //Sets the opertor for the expression
                LogicalOperator.And,
                //add the expressions
                leftComparisionExpression,
                rightComparisionExpression,
                //since this is the parent expressions there is no parent to indicate here so set it to null
                null);

            //set the contraints in the query
            query.Constraints = logicalExpression;
            //set the expression type for the query constraints
            query.Constraints.ExpressionType = ExpressionType.Logical;

            //set the parent expression for the right and left comparison expressions since they are now part of the logical expression
            leftComparisionExpression.ParentExpression = query.Constraints;
            rightComparisionExpression.ParentExpression = query.Constraints;

            var queryResults = _sysConnector.ExecuteQuery(query);

            //force a check of the query results
            foreach (var queryResult in queryResults)
            {
                break;
            }

            //validate that results have been returned
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            //validate that data has been returned
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);

            //Verify the proper first an last values returned by the query
            Assert.AreEqual("5500", queryResults.First().Properties["UnitPrice"].ToString());
            Assert.AreEqual("150", queryResults.Last().Properties["UnitPrice"].ToString());
        }

        #endregion

        #region Relationship Query Tests

        [TestMethod]
        public void SimpleChildParentValidTest()
        {
            string objectName = "Addresses";

            //add the root entity
            var rootEntity = new QueryEntity();
            rootEntity.Name = objectName;
            rootEntity.ObjectDefinitionFullName = objectName;
            //add columns to the root entity
            rootEntity.PropertyList.Add("CustomerNumber");
            rootEntity.PropertyList.Add("City");
            rootEntity.PropertyList.Add("State");
            rootEntity.PropertyList.Add("CreatedOn");

            //add the parent entity
            var parentEntity = new QueryEntity();
            parentEntity.Name = "Customers";
            parentEntity.ObjectDefinitionFullName = "Customers";
            //add columns to the parent entity
            parentEntity.PropertyList.Add("CustomerNumber");
            parentEntity.PropertyList.Add("ContactName");
            parentEntity.PropertyList.Add("Phone");
            parentEntity.PropertyList.Add("CreatedOn");
            //add the realtionship to the query
            parentEntity.RelationshipToParent = new Relationship
                                                    {
                                                        ParentProperties = "CustomerNumber", 
                                                        ChildProperties = "CustomerNumber"
                                                    };

            rootEntity.ChildList.Add(parentEntity);
            parentEntity.ParentQueryEntity = rootEntity;

            rootEntity.ParentQueryEntity = null;

            var query = new Query { IsTestQuery = false, RootEntity = rootEntity };

            //retrieve the number of columns prior to running the query
            //after the query is run verify that no columns have been added
            int rootPropertyCount = query.RootEntity.PropertyList.Count;
            int childPropertyCount = query.RootEntity.ChildList.ElementAt(0).PropertyList.Count;

            var queryResults = _sysConnector.ExecuteQuery(query);

            foreach (var queryResult in queryResults)
            {
                break;
            }

            //verify that data has been returned from the query
            Assert.IsNotNull(queryResults);
            Assert.AreNotEqual(0, queryResults.Count());
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Properties.Count);
            //verify that the first element has related values
            Assert.AreNotEqual(0, queryResults.ElementAt(0).Children.Count);

            //verify that no columns have been added
            //***Note this is a requirement of the system
            //   If a value is added during the query to ensure uniqueness of the results, 
            //   they cannot be returned in the query results
            Assert.AreEqual(rootPropertyCount, queryResults.ElementAt(0).Properties.Count);
            Assert.AreEqual(childPropertyCount, queryResults.ElementAt(0).Children.ElementAt(0).Value.ElementAt(0).Properties.Count);
        }

        [TestMethod]
        public void SelfJoinValidTest()
        {
            string objectName = "SalesOrders";
            //add the root entity
            var rootEntity = new QueryEntity();
            rootEntity.Name = objectName;
            rootEntity.ObjectDefinitionFullName = objectName;
            //add columnc to the root entity
            rootEntity.PropertyList.Add("RecordId");
            rootEntity.PropertyList.Add("CustomerNumber");
            rootEntity.PropertyList.Add("OriginalOrderNumber");
            rootEntity.PropertyList.Add("Type");
            rootEntity.PropertyList.Add("Status");

            //add the entity for the self join
            var selfJoinEntity = new QueryEntity();
            selfJoinEntity.Name = "SalesOrders2";
            selfJoinEntity.ObjectDefinitionFullName = "SalesOrders";
            
            //add the columns for the self join
            selfJoinEntity.PropertyList.Add("RecordId");
            selfJoinEntity.PropertyList.Add("Type");
            selfJoinEntity.PropertyList.Add("Status");
            selfJoinEntity.PropertyList.Add("OrderNumber");
            //add the relationship for the self join
            selfJoinEntity.RelationshipToParent = new Relationship { ParentProperties = "OriginalOrderNumber", ChildProperties = "OrderNumber" };
            rootEntity.ChildList.Add(selfJoinEntity);
            selfJoinEntity.ParentQueryEntity = rootEntity;

            rootEntity.ParentQueryEntity = null;

            var query = new Query();
            query.IsTestQuery = false;
            query.RootEntity = rootEntity;

            //retrieve the number of columns prior to running the query
            //after the query is run verify that no columns have been added
            var rootEntityPropertyCount = rootEntity.PropertyList.Count;
            var childEntityPropertyCount = selfJoinEntity.PropertyList.Count;

            var queryResults = _sysConnector.ExecuteQuery(query);

            foreach (var queryResult in queryResults)
            {
                break;
            }

            Assert.IsNotNull(queryResults);
            //there are no data in this object
            Assert.AreNotEqual(0, queryResults.Count());
            //make sure that duplicates have not been received
            Assert.AreEqual(1, queryResults.ElementAt(0).Children.ElementAt(0).Value.Count);

            //check that no extra properties have been added
            Assert.AreEqual(rootEntityPropertyCount, queryResults.ElementAt(0).Properties.Count);
            Assert.AreEqual(childEntityPropertyCount, queryResults.ElementAt(0).Children.ElementAt(0).Value.ElementAt(0).Properties.Count);
        }
        #endregion
    }
}
