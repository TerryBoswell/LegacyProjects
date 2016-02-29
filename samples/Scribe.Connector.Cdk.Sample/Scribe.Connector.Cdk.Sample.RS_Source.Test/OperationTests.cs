using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Cryptography;

namespace Scribe.Connector.Cdk.Sample.RS_Source.Test
{
    [TestClass]
    public class OperationTests
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
            OperationInput operationInput = new OperationInput {Input = new DataEntity[1]};
            //set the first item in the input property
            operationInput.Input[0] = entity;
            //set the name of the operation
            operationInput.Name = "Delete";
            //create the comparison experssion for selecting the records to delete
            ComparisonExpression comparisonExpression = new ComparisonExpression();
            comparisonExpression.ExpressionType = ExpressionType.Comparison;
            comparisonExpression.Operator = ComparisonOperator.Less;
            comparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Constant, Value = "ModifiedOn" };
            comparisonExpression.RightValue = new ComparisonValue { ValueType = ComparisonValueType.Variable, Value = DateTime.Now };
            operationInput.LookupCondition[0] = comparisonExpression;
            //execute the operation from the connector
            OperationResult operationResult = _rsSourceConnector.ExecuteOperation(operationInput);
            //validate that the operation was a success
            Assert.IsTrue(operationResult.Success[0]);
        }

        /// <summary>
        /// This is a negative test against not adding a LastSyncDate entity property
        /// </summary>
        [TestMethod]
        public void DeleteInvalidDatePropertyTest()
        {
            //create a new data entity
            DataEntity entity = new DataEntity();

            //create a new operation input with a new entity array for the input property
            OperationInput operationInput = new OperationInput { Input = new DataEntity[1] };
            //set the first item in the input property
            operationInput.Input[0] = entity;

            //set the name of the operation
            operationInput.Name = "Delete";
            //create the comparison experssion for selecting the records to delete
            ComparisonExpression comparisonExpression = new ComparisonExpression();
            comparisonExpression.ExpressionType = ExpressionType.Comparison;
            comparisonExpression.Operator = ComparisonOperator.Less;
            comparisonExpression.LeftValue = new ComparisonValue { ValueType = ComparisonValueType.Constant, Value = "ModifiedOn" };
            //note: the invalid property where the date should be
            comparisonExpression.RightValue = new ComparisonValue { ValueType = ComparisonValueType.Variable, Value = InvalidPropertyValue };
            operationInput.LookupCondition[0] = comparisonExpression;
            //execute the operation from the connector
            OperationResult operationResult = _rsSourceConnector.ExecuteOperation(operationInput);
            //validate that the operation was not a success
            Assert.IsFalse(operationResult.Success[0]);
            //validate that the error info is filled in
            Assert.IsNotNull(operationResult.ErrorInfo[0]);
        }

        #endregion
    }
}
