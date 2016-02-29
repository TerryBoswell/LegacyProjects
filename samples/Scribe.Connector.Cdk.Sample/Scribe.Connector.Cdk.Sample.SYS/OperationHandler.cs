// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationHandler.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2012 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Core.ConnectorApi.Query;

namespace Scribe.Connector.Cdk.Sample.SYS
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
        /// <param name="operationInput">The operation information being executed.</param>
        /// <returns>The result of the operation that was processed.</returns>
        public OperationResult DeleteOperation(OperationInput operationInput)
        {
            OperationResult operationResult = new OperationResult();

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point 
            // is written during garbage collection.
            using (new LogMethodExecution(Globals.ConnectorName, "Delete"))
            {
                // Each record processed must return the following 3 pieces of information.
                // Each piece of information should be added in the same order it was received.
                // The first piece of information would be whether or not the request was successful.
                // If the requested record does not exist it should not result in a failure.
                List<bool> successList = new List<bool>();
                // The second piece of information is the number of records that have been processed.
                // If a delete is attempted on a record that does not exist then 0 rows should be reported here.
                List<int> objectList = new List<int>();
                // In the event of an error during processing the record, error information should be added here.
                // If no error occured a null placeholder for that record is expected.
                List<ErrorResult> errors = new List<ErrorResult>();

                int index = 0;
                // Execute each of the inputs individually
                // **** Processing inputs individually is done because the 
                //      connector is responsible for error handling on each.
                //      The connector must return results in the same order in which the 
                //      data entities were received, this allows for reprocessing of failed records.
                //Note: If the SupportsBulk flag is not set in the ActionDefinition 
                //      that corresponds to this operation, operationInput.Input 
                //      will always have always have a length of 1.
                foreach (DataEntity inputEntity in operationInput.Input)
                {
                    try
                    {
                        // Process the number of rows that will be deleted.
                        ValidateRowCount(inputEntity, operationInput.LookupCondition[index],
                                                   operationInput.AllowMultipleObject);

                        // Use the query builder to parse input conditions.
                        var query = new SqlQueryBuilder(inputEntity, operationInput.LookupCondition[index], Globals.QueryType.Delete);

                        // Execute the query generated from the operation input.
                        int rowCount = _dataAccess.ExecuteNonQuery(query.ToString());

                        // Add a the result to the result list.
                        successList.Add(SetSuccessResult(operationInput.AllowMultipleObject, rowCount));
                        objectList.Add(rowCount);
                        errors.Add(SetErrorResult(rowCount));
                        index++;

                    }
                    catch (ArgumentException argumentException)
                    {
                        // This will catch a filter that returns multiple rows
                        // when only one is expected.
                        var errorResult = new ErrorResult()
                        {
                            Description = argumentException.Message,
                            Number = ErrorCodes.TooManyRowsReturned.Number
                        };

                        errors.Add(errorResult);
                        successList.Add(false);
                        objectList.Add(0);
                    }
                    catch (Exception exception)
                    {
                        // In the event of an exception do not stop performing 
                        // all operations simply log each individually
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
        /// Perform the Create operations on the selected table.
        /// This method will filter creations using the 
        /// SqlQueryBuilder and the lookup conditions
        /// </summary>
        /// <param name="operationInput">The operation information being executed.</param>
        /// <returns>The result of the operation that was processed.</returns>
        public OperationResult CreateOperation(OperationInput operationInput)
        {
            OperationResult operationResult = new OperationResult();

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point
            // is written during garbage collection.
            using (new LogMethodExecution(Globals.ConnectorName, "Create"))
            {
                // Each record processed must return the following 3 pieces of information.
                // Each piece of information should be added in the same order it was received.
                // The first piece of information would be whether or not the request was successful.
                // If the requested record that has a key that already exists this should not result in a failure.
                List<bool> successList = new List<bool>();
                // The second piece of information is the number of records that have been processed.
                // If a duplicate key is detected when performing an insert then 0 rows should be added for the request.
                List<int> objectList = new List<int>();
                // In the event of an error during processing the record, error information should be added here.
                // If no error occured a null placeholder for that record is expected.
                List<ErrorResult> errors = new List<ErrorResult>();
                
                //Execute each of the inputs individually
                // **** Processing inputs individually is done because the 
                //      connector is responsible for error handling on each.
                //      The connector must return results in the same order in which the 
                //      data entities were received, this allows for reprocessing of failed records.
                //Note: If the SupportsBulk flag is not set in the ActionDefinition 
                //      that corresponds to this operation, operationInput.Input 
                //      will always have always have a length of 1.
                foreach (DataEntity inputEntity in operationInput.Input)
                {
                    try
                    {
                        //Use the query builder to parse input conditions
                        SqlQueryBuilder queryBuilder = new SqlQueryBuilder(
                            inputEntity, Globals.QueryType.Insert);

                        //execute the create query
                        int rowsEffected = _dataAccess.ExecuteNonQuery(queryBuilder.ToString());

                        //Add the result of the create to the result lists
                        successList.Add(SetSuccessResult(operationInput.AllowMultipleObject, rowsEffected));
                        objectList.Add(rowsEffected);
                        errors.Add(SetErrorResult(rowsEffected));
                    }
                    catch (OleDbException oleDbException)
                    {
                        //Create a new error result for ole db specific exeptions
                        ErrorResult errorResult = new ErrorResult();

                        var oleDbError = oleDbException.ErrorCode;
                        //Look for a specific error code that occurs when attempting to duplicate a record.
                        //This will tell ScribeOnline that an update is required rather than an Insert.
                        if (oleDbError == -2147217873)
                        {
                            //this is the error code for a 'Violation in unique index'
                            errorResult.Number = ErrorNumber.DuplicateUniqueKey;
                            if (oleDbException.Errors[0] != null && oleDbException.Errors[0] != null)
                            {
                                var dbError = oleDbException.Errors[0];
                                errorResult.Description = dbError != null ? dbError.Message : oleDbException.Message;

                                var error = oleDbException.Errors[1];
                                errorResult.Detail = error != null ? error.Message : oleDbException.StackTrace;
                            }
                        }
                        else
                        {
                            errorResult.Description = oleDbException.Message;
                            errorResult.Detail = oleDbException.StackTrace;
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
        /// <param name="operationInput">The operation information being executed.</param>
        /// <returns>The result of the operation that was processed.</returns>
        public OperationResult UpdateOperation(OperationInput operationInput)
        {
            OperationResult operationResult = new OperationResult();

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point
            // is written during garbage collection.
            using (new LogMethodExecution(Globals.ConnectorName, "Update"))
            {
                // Each record processed must return the following 3 pieces of information.
                // Each piece of information should be added in the same order it was received.
                // The first piece of information would be whether or not the request was successful.
                // If the requested record does not exist this should not result in a failure.
                List<bool> successList = new List<bool>();
                // The second piece of information is the number of records that have been processed.
                // If the record requested for update does no exist then 0 rows should be added here.
                List<int> objectList = new List<int>();
                // In the event of an error during processing the record, error information should be added here.
                // If no error occured a null placeholder for that record is expected.
                List<ErrorResult> errors = new List<ErrorResult>();

                int index = 0;
                //Execute each of the inputs individually
                // **** Processing inputs individually is done because the 
                //      connector is responsible for error handling on each.
                //      The connector must return results in the same order in which the 
                //      data entities were received, this allows for reprocessing of failed records.
                //Note: If the SupportsBulk flag is not set in the ActionDefinition 
                //      that corresponds to this operation, operationInput.Input 
                //      will always have always have a length of 1.
                foreach (DataEntity inputEntity in operationInput.Input)
                {
                    try
                    {
                        //process the number of rows that will be effected by the update operation
                        ValidateRowCount(inputEntity, operationInput.LookupCondition[index],
                                                   operationInput.AllowMultipleObject);

                        //Use the query builder to parse input conditions
                        string updateQuery = new SqlQueryBuilder(
                            inputEntity, operationInput.LookupCondition[index], Globals.QueryType.Update).ToString();

                        //Execute the update based on the select filter
                        int rowCount = _dataAccess.ExecuteNonQuery(updateQuery);

                        //Add the result of the update to the result lists
                        //set the appropriate success results, If multiple records were returned but the operation did not allow multiples
                        //then the operation was not successfull.
                        successList.Add(SetSuccessResult(operationInput.AllowMultipleObject, rowCount));
                        objectList.Add(rowCount);
                        errors.Add(SetErrorResult(rowCount));
                        index++;

                    }
                    catch (Exception exception)
                    {
                        //In the event of an exception do not stop performing all operations
                        //simple log each individually
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
        /// Perform the Upsert operations on the selected table. The connector will first identify if the DataEntity
        /// already exists in the data source and then perform either an update or insert operation based on the results.
        /// This method will filter updates using the 
        /// SqlQueryBuilder and the lookup conditions
        /// </summary>
        /// <param name="operationInput">The operation information being executed.</param>
        /// <param name="metadataAccess">Metadata associated with the active connection.</param>
        /// <returns>The result of the operation that was processed.</returns>
        public OperationResult UpsertOperation(OperationInput operationInput, OleDbMetadataAccess metadataAccess)
        {
            OperationResult operationResult = new OperationResult();

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point
            // is written during garbage collection.
            using (new LogMethodExecution(Globals.ConnectorName, "Upsert"))
            {
                // Each record processed must return the following 3 pieces of information.
                // Each piece of information should be added in the same order it was received.
                // The first piece of information would be whether or not the request was successful.
                List<bool> successList = new List<bool>();
                // The second piece of information is the number of records that have been processed.
                List<int> objectList = new List<int>();
                // In the event of an error during processing the record, error information should be added here.
                // If no error occured a null placeholder for that record is expected.
                List<ErrorResult> errors = new List<ErrorResult>();

                //Execute each of the inputs individually
                // **** Processing inputs individually is done because the 
                //      connector is responsible for error handling on each.
                //      The connector must return results in the same order in which the 
                //      data entities were received, this allows for reprocessing of failed records.
                //Note: If the SupportsBulk flag is not set in the ActionDefinition 
                //      that corresponds to this operation, operationInput.Input 
                //      will always have always have a length of 1.
                foreach (DataEntity inputEntity in operationInput.Input)
                {
                    try
                    {
                        var primaryKeyPropertes = GetPrimaryKeyProperties(inputEntity, metadataAccess);

                        //Generate the query to perform the upsert
                        var upsertQuery =
                            new SqlQueryBuilder(inputEntity, primaryKeyPropertes, Globals.QueryType.Upsert);

                        //execute the upsert query
                        int rowsEffected = _dataAccess.ExecuteNonQuery(upsertQuery.ToString());

                        //Add the result of the update to the result lists
                        //set the appropriate success results, If multiple records were returned but the operation did not allow multiples
                        //then the operation was not successfull.
                        successList.Add(SetSuccessResult(operationInput.AllowMultipleObject, rowsEffected));
                        objectList.Add(rowsEffected);
                        errors.Add(SetErrorResult(rowsEffected));
                    }
                    catch (Exception exception)
                    {
                        //In the event of an exception do not stop performing all operations
                        //simple log each individually
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

        #endregion

        #region private methods

        /// <summary>
        /// Retrieves the number of rows based on the entity and lookup condition provided.
        /// This method will also throw an ArgumentException if multiple rows are found in the datasource
        /// based on the lookup condition when the allowMultiples flag was not set.
        /// </summary>
        /// <param name="dataEntity">Data entity used to retrieve the row count.</param>
        /// <param name="lookupCondition">LookupCondition that is equivlent to a sql 'where' clause.</param>
        /// <param name="allowMultiples">Flag that identifies if multple rows may be effected by a single query.</param>
        /// <returns></returns>
        private void ValidateRowCount(DataEntity dataEntity, Expression lookupCondition, bool allowMultiples)
        {
            //create the select count (*) query
            var selectQuery = new SqlQueryBuilder(dataEntity, lookupCondition, Globals.QueryType.Count);

            //execute the the count query
            var queryResults = _dataAccess.Execute(selectQuery.ToString());

            //retrieve the row count from the query
            var rowCount = Convert.ToInt32(queryResults.Rows[0][0]);

            //validate whether or not more than one row will be effected
            if (allowMultiples == false && rowCount > 1)
            {
                throw new ArgumentOutOfRangeException("allowMultiples", string.Format(ErrorCodes.TooManyRowsReturned.Description, rowCount));
            }
        }

        private EntityProperties GetPrimaryKeyProperties(DataEntity dataEntity, OleDbMetadataAccess metadataAccess)
        {
            var primaryKeyProperties = new EntityProperties();
            //Use the data entity name to retrieve a list of indexes 
            var indexColumns = metadataAccess.GetTableIndexInformation(dataEntity.ObjectDefinitionFullName);

            //Add each of the Primary Keys and their values found in the data entity.
            foreach (DataRow row in indexColumns.Rows)
            {
                if (!Convert.ToBoolean(row["PRIMARY_KEY"]))
                {
                    continue;
                }

                var columnName = row["COLUMN_NAME"].ToString();

                // Check if the priamry key column is included in the data entity.
                if (dataEntity.Properties.ContainsKey(columnName))
                {
                    // Add the key and its value to the primary key list.
                    primaryKeyProperties.Add(columnName, dataEntity.Properties[columnName]);
                }
                else
                {
                    // If the key has not been added set it to null.
                    primaryKeyProperties.Add(columnName, null);
                }
            }

            return primaryKeyProperties;
        }


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

        /// <summary>
        /// Set the success value based on whether rows were updated and if multi row updates are allowed
        /// </summary>
        /// <param name="allowMultiple">true if this allows multiple row updates</param>
        /// <param name="rowsUpdated">number of rows updated</param>
        /// <returns>true for success, fales if not successfull</returns>
        private bool SetSuccessResult(bool allowMultiple, int rowsUpdated)
        {
            bool success;

            //check if multiple rows have been returned, and set the success appropriatly;
            if (!allowMultiple)
            {
                success = rowsUpdated <= 1;
            }
            else
            {
                success = true;
            }

            return success;
        }
        #endregion
    }
}
