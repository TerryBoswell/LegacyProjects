// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationHandler.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Logger;

namespace Scribe.Connector.Cdk.Sample.RS_Source
{
    /// <summary>
    /// This is the class that will handle all operations for the connector. 
    /// The only operation required in a source connector is the 'Delete' operation.
    /// </summary>
    public class OperationHandler
    {
        //stores the current instance of the data access layer to use the current connection to access the metadata
        private readonly OleDbDataAccess _dataAccess;
        //this is the message to indicate an invalid property name in the operation input
        private const string InputPropertyNotFound = "Input property not found in properties list";
        #region ctor
        /// <summary>
        /// Constructor for the Operation Handler
        /// </summary>
        /// <param name="dataAccess"></param>
        public OperationHandler(OleDbDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Perform the Delete operation on the created ScribeChangeHistory table
        /// Note: If the data-source already has a process for tracking changes, this 
        ///       method will only need to return a positive success in the operation result
        /// </summary>
        /// <param name="operationInput"></param>
        /// <returns></returns>
        public OperationResult DeleteOperation(OperationInput operationInput)
        {
            OperationResult operationResult = new OperationResult();

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point 
            // is written during garbage collection.
            using (new LogMethodExecution(Globals.ConnectorName, "Delete"))
            {
                List<bool> successList = new List<bool>();
                List<int> objectList = new List<int>();
                List<ErrorResult> errors = new List<ErrorResult>();

                int index = 0;
                //Execute each of the inputs individually
                foreach (DataEntity inputEntity in operationInput.Input)
                {
                    try
                    {
                        //Construct the query that will remove any records in the change 
                        //history table that were used in a previous syncronization
                        /*Note: 
                         * A more enhanced example of query parsing can be found in 
                         * SqlQueryBuilder.cs which is part of the Sample RS Target Connector
                        */
                        string query = string.Format("DELETE FROM ScribeChangeHistory {0}",
                             ParseComparisionExpression(
                             operationInput.LookupCondition[index] as ComparisonExpression));
                        //execute the query to clean the scribe change history table
                        int recordsDeleted = _dataAccess.ExecuteNonQuery(query);
                        //add a new success result
                        successList.Add(true);
                        objectList.Add(recordsDeleted);
                        errors.Add(new ErrorResult());
                        index++;
                    }
                    catch (Exception exception)
                    {
                        //In the event of an exception do not stop performing all operations
                        //simple log each individually
                        successList.Add(false);
                        objectList.Add(0);
                        errors.Add(new ErrorResult
                        { Description = exception.Message, Detail = exception.ToString() });
                    }
                }

                //Add the results from the operations to the operation result object
                operationResult.Success = successList.ToArray();
                operationResult.ObjectsAffected = objectList.ToArray();
                operationResult.ErrorInfo = errors.ToArray();
            }

            return operationResult;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Parse through the expression to convert it to a query
        /// </summary>
        /// <param name="expression">Comparison Expression from OperationInpit.LookupCondition</param>
        /// <returns>return the converted expression</returns>
        private string ParseComparisionExpression(ComparisonExpression expression)
        {
            string comparisonString = string.Empty;
            //parse the expression and throw an exception if it is unsupported
            switch (expression.Operator)
            {
                case ComparisonOperator.Equal:
                    comparisonString = "=";
                    break;
                case ComparisonOperator.Less:
                    comparisonString = "<";
                    break;
                case ComparisonOperator.LessOrEqual:
                    comparisonString = "<=";
                    break;
                default:
                    throw new NotSupportedException(string.Format("Operation Not Supported : {0}", expression.Operator));
            }
            //Get the last sync date from the expression right value
            DateTime lastSyncDate = Convert.ToDateTime(expression.RightValue.Value);
            //return the converted expression
            return string.Format("WHERE ModifiedOn {0} '{1}'", comparisonString, lastSyncDate.ToString("u").TrimEnd('Z'));
        }
        #endregion
    }
}
