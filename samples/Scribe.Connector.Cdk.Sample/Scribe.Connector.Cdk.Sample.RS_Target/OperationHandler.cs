// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationHandler.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.OleDb;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Logger;

namespace Scribe.Connector.Cdk.Sample.RS_Target
{
    internal class OperationHandler
    {
        #region private members
        //stores the current instance of the data access layer to use the current connection to access the metadata
        private readonly OleDbDataAccess _dataAccess;

        #endregion

        #region ctor
        public OperationHandler(OleDbDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Perform the Delete operations on the selected table.
        /// This method will filter deletes using the SqlQueryBuilder and the lookup conditions
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
                        //use the query builder to parse input conditions
                        SqlQueryBuilder queryBuilder = new SqlQueryBuilder(
                            inputEntity, operationInput.LookupCondition[index], 
                            Globals.OperationType.Delete);

                        //Execute the query generated from the operation input.
                        int rowsEffected = _dataAccess.ExecuteNonQuery(queryBuilder.ToString());

                        //Add a the result to the result list.
                        successList.Add(rowsEffected >= 1);
                        objectList.Add(rowsEffected);
                        errors.Add(SetErrorResult(rowsEffected));
                        index++;

                    }
                    catch (Exception exception)
                    {
                        // In the event of an exception do not stop performing 
                        // all operations simply log each individually
                        successList.Add(false);
                        objectList.Add(0);
                        errors.Add(new ErrorResult() 
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

        /// <summary>
        /// Perform the Create operations on the selected table.
        /// This method will filter creations using the 
        /// SqlQueryBuilder and the lookup conditions
        /// </summary>
        /// <param name="operationInput"></param>
        /// <returns></returns>
        public OperationResult CreateOperation(OperationInput operationInput)
        {
            OperationResult operationResult = new OperationResult();

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point
            // is written during garbage collection.
            using (new LogMethodExecution(Globals.ConnectorName, "Create"))
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
                        //Use the query builder to parse input conditions
                        SqlQueryBuilder queryBuilder = new SqlQueryBuilder(
                            inputEntity, null,
                            Globals.OperationType.Create);

                        //execute the create query
                        int rowsEffected = _dataAccess.ExecuteNonQuery(queryBuilder.ToString());

                        //Add the result of the create to the result lists
                        successList.Add(rowsEffected == 1);
                        objectList.Add(rowsEffected);
                        errors.Add(SetErrorResult(rowsEffected));
                        index++;

                    }
                    catch (OleDbException oleDbException)
                    {
                        //Create a new error result for ole db specific exeptions
                        ErrorResult errorResult = new ErrorResult
                        {
                            Description = oleDbException.Message,
                            Detail = oleDbException.StackTrace
                        };

                        var oleDbError = oleDbException.ErrorCode;
                        //Look for a specific error code that occurs when attempting to duplicate a record.
                        //This will tell ScribeOnline that an update is required rather than an Insert.
                        if (oleDbError == -2147217873)
                        {
                            //this is the error code for a 'Violation in unique index'
                            errorResult.Number = ErrorNumber.DuplicateUniqueKey;
                        }
                        else
                        {
                            errorResult.Number = oleDbError;
                        }
                        successList.Add(false);
                        objectList.Add(0);
                        errors.Add(errorResult);
                    }
                    catch (Exception exception)
                    {
                        //In the event of an exception do not stop performing 
                        //all operations simply log each individually.
                        successList.Add(false);
                        objectList.Add(0);
                        errors.Add(new ErrorResult() { Description = exception.Message, Detail = exception.ToString() });
                    }
                }

                //Add the results from the operations to the operation result object
                operationResult.Success = successList.ToArray();
                operationResult.ObjectsAffected = objectList.ToArray();
                operationResult.ErrorInfo = errors.ToArray();
            }
            return operationResult;
        }

        /// <summary>
        /// Perform the Update operations on the selected table.
        /// This method will filter updates using the 
        /// SqlQueryBuilder and the lookup conditions
        /// </summary>
        /// <param name="operationInput"></param>
        /// <returns></returns>
        public OperationResult UpdateOperation(OperationInput operationInput)
        {
            OperationResult operationResult = new OperationResult();

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point
            // is written during garbage collection.
            using (new LogMethodExecution(Globals.ConnectorName, "Update"))
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
                        //Use the query builder to parse input conditions
                        SqlQueryBuilder queryBuilder = new SqlQueryBuilder(
                            inputEntity, operationInput.LookupCondition[index], 
                            Globals.OperationType.Update);

                        //Execute the update based on the select filter
                        int rowsEffected = _dataAccess.ExecuteNonQuery(queryBuilder.ToString());
                        
                        //Add the result of the updat to the result lists
                        successList.Add(rowsEffected >= 1);
                        objectList.Add(rowsEffected);
                        errors.Add(SetErrorResult(rowsEffected));
                        index++;

                    }
                    catch (Exception exception)
                    {
                        //In the event of an exception do not stop performing all operations
                        //simple log each individually
                        successList.Add(false);
                        objectList.Add(0);
                        errors.Add(new ErrorResult() 
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
        /// Set the error result of the operation bassed on how many rows have been deleted
        /// </summary>
        /// <param name="rowsEffected"></param>
        /// <returns></returns>
        private ErrorResult SetErrorResult(int rowsEffected)
        {
            ErrorResult errorResult = new ErrorResult();

            if (rowsEffected == 0)
            {
                errorResult.Number = ErrorCodes.NoRowsFound.Number;
                errorResult.Description = ErrorCodes.NoRowsFound.Description;
            }

            return errorResult;
        }


        #endregion
    }
}
