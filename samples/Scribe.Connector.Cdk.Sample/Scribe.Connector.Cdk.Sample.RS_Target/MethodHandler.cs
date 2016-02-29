// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodHandler.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Data.OleDb;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Core.ConnectorApi.Query;
using Scribe.Core.ConnectorApi.Metadata;

namespace Scribe.Connector.Cdk.Sample.RS_Target
{
    using System.Collections.Generic;
    using System.Data;

    internal class MethodHandler
    {
        #region private members
        //stores the current instance of the data access layer to use the current connection to access the metadata
        private readonly OleDbDataAccess _dataAccess;
        //the field to use to track when a record has changed
        private const string LastModifiedFieldName = "ModifiedOn";
        #endregion

        #region ctor
        /// <summary>
        /// Constructor for a new method handler
        /// </summary>
        /// <param name="oleDbDataAccess">the current open instance of the dataaccess layer</param>
        public MethodHandler(OleDbDataAccess oleDbDataAccess)
        {
            //store the current data access instance localy
            _dataAccess = oleDbDataAccess;
        }
        #endregion

        #region public methods

        /// <summary>
        /// Retrieve the last date the specified table was syncronized
        /// MethodInput.Input.Property.Keys 
        /// "ObjectName": Name of the table
        /// "ModificationDateFullName": Name of the column that stored the last date of syncronization
        /// </summary>
        /// <param name="methodInput">Container for the methods input properties</param>
        /// <returns>MethodResult</returns>
        public MethodResult GetLastReplicationSyncDate(MethodInput methodInput)
        {
            MethodResult methodResult = new MethodResult();

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point is written during garbage collection.
            using (new LogMethodExecution(
                Globals.ConnectorName, "GetLastReplicationSyncDate"))
            {
                //get the name of the table from the method input
                string tableName = GetPropertyValue(
                    "ObjectName", methodInput.Input.Properties);

                string modifiedOnColumn = GetPropertyValue(
                    "ModificationDateFullName",
                    methodInput.Input.Properties);

                DateTime lastSyncDate;

                try
                {
                    //verify that the column name for the lasty sync date is specified
                    if (string.IsNullOrWhiteSpace(modifiedOnColumn) == false)
                    {
                        string query = string.Format(
                            "SELECT TOP 1 [{0}] FROM [{1}] ORDER BY [{0}] DESC", modifiedOnColumn, tableName);
                        //execute the query
                        DataTable lastSyncDateTable = _dataAccess.Execute(query);
                        if (lastSyncDateTable.Rows.Count != 0)
                        {
                            lastSyncDate = Convert.ToDateTime(
                                lastSyncDateTable.Rows[0][modifiedOnColumn]);
                        }
                        //If no records are found in the table then set the last sync date to the min value
                        else
                        {
                            lastSyncDate = DateTime.MinValue;
                        }
                    }
                    //If no last sync date column is specified set the last sync date to the min value
                    else
                    {
                        lastSyncDate = DateTime.MinValue;
                    }
                    //create a new method result
                    methodResult = new MethodResult { Success = true, Return = new DataEntity("ReturnValue") };
                    //add the LastSyncDate to the return properties
                    methodResult.Return.Properties.Add("LastSyncDate", lastSyncDate);
                    //put the last sync date in the debug log
                    Logger.Write(Logger.Severity.Debug, Globals.ConnectorName,
                        "LastSyncDate: " + lastSyncDate + Environment.NewLine);
                }
                //catch an errors comming from the database
                //All other erros will be caught on a higher level and thrown
                //As an InvalidExecuteMethodException from the connector interface
                catch (OleDbException oleDbException)
                {
                    //in the event of a database error create a 
                    //new method result and set the succes to false
                    methodResult = new MethodResult { Success = false };
                    //create the new error info and set the number 
                    //to the code from the connection
                    methodResult.ErrorInfo = new ErrorResult { Number = oleDbException.ErrorCode };
                    //create the description for the error
                    methodResult.ErrorInfo.Description =
                        oleDbException.Message;
                    methodResult.ErrorInfo.Detail =
                        oleDbException.ToString();
                }
            }

            return methodResult;
        }

        /// <summary>
        /// This method determines whether or not an object from the source has changed.
        /// If the object does not exist it will be created.
        /// If the object has changed it will be deleted and the new one will be created.
        /// If no changes are detected then it will be noted and no further action is needed.
        /// </summary>
        /// <param name="methodInput"></param>
        /// <returns></returns>
        public MethodResult CreateOrUpdateObjectForReplication(MethodInput methodInput)
        {
            MethodResult methodResult = new MethodResult();

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point 
            //is written during garbage collection.
            using (new LogMethodExecution(
                Globals.ConnectorName, "CreateOrUpdateObjectForReplication"))
            {
                string tableName = GetPropertyValue("Name", methodInput.Input.Properties);
                //get the list of the columns to be created
                List<DataEntity> newColumns =
                    methodInput.Input.Children["RSPropertyDefinitions"];

                //add the default columns to the new ones from the input
                newColumns.AddRange(CreateDefaultColumns());

                //get the definition of the existing table
                MethodResult currentTableDefinition =
                    GetObjectDefinition(new MethodInput
                                            {
                                                Input =
                                                    {
                                                        Properties = new EntityProperties
                                                                             {
                                                                                 {"ObjectName", tableName}
                                                                             }
                                                    }
                                            });

                bool changeDetected = true;

                //check if the table was returned from GetObjectDefinition method
                if (currentTableDefinition != null && currentTableDefinition.Success)
                {
                    //look for any changes in the table schema by 
                    //comparing the properties
                    //of the existing object against the those
                    //found in the method input
                    changeDetected = CheckForSchemaChanges(newColumns,
                                                           currentTableDefinition.Return.Children[
                                                               "RSPropertyDefinitions"]);

                    //if a change is decteded drop the table
                    if (changeDetected)
                    {
                        string query = string.Format("DROP TABLE [{0}]", tableName);
                        _dataAccess.ExecuteNonQuery(query);
                    }
                }

                //Ff a change is detected create the table
                if (changeDetected)
                {
                    _dataAccess.CreateTable(tableName, newColumns);
                }

                //Set the method result object
                methodResult = new MethodResult { Success = true, Return = new DataEntity() };

                //add a property called 'SchemaChanged' to the return properties
                //this is how ScribeOnline will determine whether or not a replication
                //is required when this method has completed
                //Note: this MUST be labeled 'SchemaChanged'
                methodResult.Return.Properties.Add("SchemaChanged", changeDetected);
            }

            return methodResult;
        }

        /// <summary>
        /// Get a specific Object's definition, this includes any attributes and supporting object properties.
        /// In this case retrieve the table definition along with any columns and the definition of each.
        /// </summary>
        /// <param name="methodInput">Method Input which includes an 'ObjectName' 
        /// property to determine the object to retrieve the definition for.</param>
        /// <returns>Method Result which will either include error information or the 
        /// Object Definition of the 'ObjectName' specified in the MethodInput properties</returns>
        public MethodResult GetObjectDefinition(MethodInput methodInput)
        {
            //Create a new instance of the method result to 
            //fill with meta data information
            MethodResult result = new MethodResult();

            //create a new instance of the metadata access class and 
            //pass the data access instance allong with it
            OleDbMetadataAccess metadataAccess = new OleDbMetadataAccess(_dataAccess);

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point 
            //is written during garbage collection.
            using (new LogMethodExecution(
                Globals.ConnectorName, "GetObjectDefinitionMethod"))
            {
                //get the name of the object in the input properties
                string objectName = GetPropertyValue(
                    "ObjectName", methodInput.Input.Properties);

                //using the meta data access get the definitions 
                //for each of the columns in the table
                DataTable tableColumnDefinitions =
                    metadataAccess.GetColumnDefinitions(objectName);

                //using the meta data access get the definition for 
                //the table indexes (primary and foreign keys)
                DataTable tableIndexDefinition =
                    metadataAccess.GetTableIndexInformation(objectName);

                //check that both sets of data have been 
                //returned from the meta data access layer
                if ((tableColumnDefinitions != null
                    && tableColumnDefinitions.Rows.Count != 0)
                    && (tableIndexDefinition != null
                    && tableIndexDefinition.Rows.Count != 0))
                {
                    //create a new replication service object
                    RSObjectDefinition rsObjectDefinition = new RSObjectDefinition()
                    {
                        Name = objectName,
                        RSPropertyDefinitions = new List<RSPropertyDefinition>()
                    };

                    List<string> tablePrimaryKeys =
                        GetTablePrimaryKeys(rsObjectDefinition.Name, metadataAccess);

                    //parse through each column return from the column definitions and
                    //add a new replication service property definition to the newly created
                    //replication service object definition for each column in the table
                    foreach (DataRow columnDefinition in tableColumnDefinitions.Rows)
                    {
                        //process the column definition and set it 
                        //to the resplication service property definition
                        RSPropertyDefinition rsPropertyDefinition =
                            ProcessColumnDefinition(columnDefinition);

                        //check if this is the default last 
                        //modified column and set the object property
                        if (rsPropertyDefinition.Name == LastModifiedFieldName)
                        {
                            rsObjectDefinition.ModificationDateFullName =
                                rsPropertyDefinition.Name;
                        }

                        //check if the property is a primary key value
                        rsPropertyDefinition.InPrimaryKey =
                            tablePrimaryKeys.Contains(rsPropertyDefinition.Name);

                        //add the property definition to the object definition
                        rsObjectDefinition.RSPropertyDefinitions.Add(rsPropertyDefinition);
                    }

                    //Convert the replication service object definition to a Data Entity
                    //set the result return value to the replication service object defintion
                    //set the result success to true
                    result = new MethodResult { Success = true, Return = rsObjectDefinition.ToDataEntity() };
                }
                else
                {
                    result = new MethodResult { Success = false, ErrorInfo = new ErrorResult { Description = ErrorCodes.ObjectNotFound.Description, Number = ErrorCodes.ObjectNotFound.Number } };
                }
            }

            //return the method result
            return result;
        }
        #endregion

        #region private helper methods
        /// <summary>
        /// Process the column definitions returned from the schema and convert them to use in a property definition
        /// </summary>
        /// <param name="columnDefinition">Data row return the from the database containing the definition of the current column</param>
        /// <returns>Replication Service Property Definition containing the converted column definition</returns>
        private RSPropertyDefinition ProcessColumnDefinition(DataRow columnDefinition)
        {
            //create a new property definition with the initial values in it
            RSPropertyDefinition propertyDefinition = new RSPropertyDefinition();

            //set the name of the property definition to the name of the column
            propertyDefinition.Name = columnDefinition["COLUMN_NAME"].ToString();

            //set whether the property is nullable using the nullable attribute of the column
            propertyDefinition.Nullable = Convert.ToBoolean(columnDefinition["IS_NULLABLE"]);

            //***********IMPORTANT STEP*******************
            //Convert the data type attribute of the column to a generic .Net DataType and 
            //set the DataType attribute in the propery definition.
            //Note: The String representation is used ie: "An int is stored as System.Int32"
            propertyDefinition.DataType = DataTypeConverter.OleDbToSystem(columnDefinition["DATA_TYPE"]).ToString();

            //Check if the max length is set and add it to the property definition
            if (string.IsNullOrWhiteSpace(columnDefinition["CHARACTER_MAXIMUM_LENGTH"].ToString()) == false)
            {
                propertyDefinition.MaximumLength = Convert.ToInt32(columnDefinition["CHARACTER_MAXIMUM_LENGTH"]);
            }

            //Check if the scale is set and add it to the property definition
            if (string.IsNullOrWhiteSpace(columnDefinition["NUMERIC_SCALE"].ToString()) == false)
            {
                propertyDefinition.NumericScale = Convert.ToInt32(columnDefinition["NUMERIC_SCALE"]);
            }

            //Check if the Precision is set and add it to the property definition
            if (string.IsNullOrWhiteSpace(columnDefinition["NUMERIC_PRECISION"].ToString()) == false)
            {
                propertyDefinition.NumericPrecision = Convert.ToInt32(columnDefinition["NUMERIC_PRECISION"]);
            }

            //send back the created property definition
            return propertyDefinition;
        }

        /// <summary>
        /// Get the value of the specified property
        /// </summary>
        /// <param name="properties">property to search for</param>
        /// <returns>value of the specified property</returns>
        private string GetPropertyValue(string propertyName, EntityProperties properties)
        {
            // validate that an property name has been set
            if (!properties.ContainsKey(propertyName))
            {
                throw new ArgumentNullException(propertyName, Globals.InputPropertyNotFound);
            }

            var objectName = properties[propertyName].ToString();

            return objectName;
        }

        /// <summary>
        /// In the event of an error we want to package it up nicely within the Method Result Error Infor property,
        /// as well as Log the occurence
        /// </summary>
        /// <param name="errorCode">Int value for the error code</param>
        /// <param name="errorDescription">Text value fo the description of the error</param>
        /// <returns>Method Result object containing with the error information property populated</returns>
        private MethodResult SetErrorMethodResult(int errorCode, string errorDescription)
        {
            //create a new instance of the Method Result object from the Connector Api packaging up the error code and description
            MethodResult result = new MethodResult
                                      {
                                          Success = false,
                                          ErrorInfo =
                                              new ErrorResult { Number = errorCode, Description = errorDescription }
                                      };

            //Use the Connector Api Logger to log the error event, include the severity level, connector name as well as the description
            Logger.Write(Logger.Severity.Error, Globals.ConnectorName, result.ErrorInfo.Description);

            //send back the method result object that was created
            return result;
        }

        /// <summary>
        /// Get the list of primary key names in the table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="metadataAccess"></param>
        /// <returns>LIst of primary keys for the specified table</returns>
        private List<string> GetTablePrimaryKeys(string tableName, OleDbMetadataAccess metadataAccess)
        {
            List<string> primaryKeys = new List<string>();
            //get the list of indexes for the table
            DataTable indexList = metadataAccess.GetTableIndexInformation(tableName);

            //check that the table even has an index
            if (indexList != null && indexList.Rows.Count != 0)
            {
                //find the primary key values and add them to the list
                foreach (DataRow index in indexList.Rows)
                {
                    if (Convert.ToBoolean(index["PRIMARY_KEY"]))
                    {
                        primaryKeys.Add(index["COLUMN_NAME"].ToString());
                    }
                }
            }

            return primaryKeys;
        }

        /// <summary>
        /// Create a list of default columns.
        /// Four default columns are create.
        /// 1: [SCRIBE_ID] This is the new primary key for the table
        /// 2: [SCRIBE_CREATEDON] The date of row creation
        /// 3: [SCRIBE_MODIFIEDON] The date of a rows last modification
        /// 4: [SCRIBE_DELETEDON] The date of row deletion
        /// </summary>
        /// <returns>The Created Scribe Specific replicaiton columns</returns>
        private List<DataEntity> CreateDefaultColumns()
        {
            List<DataEntity> defaultCols = new List<DataEntity>();

            //Create primary key column
            DataEntity column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", Globals.ScribePrimaryKey);
            column.Properties.Add("DataType", "System.Int64");
            column.Properties.Add("Identity", true);
            column.Properties.Add("IdentitySeed", 1);
            column.Properties.Add("IdentityIncrement", 1);
            column.Properties.Add("Nullable", false);
            column.Properties.Add("InPrimaryKey", true);
            defaultCols.Add(column);

            //add created on standard column
            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", Globals.ScribeCreatedOn);
            column.Properties.Add("DataType", "System.DateTime");
            column.Properties.Add("Nullable", false);
            defaultCols.Add(column);

            //add modified on standard column
            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", Globals.ScribeModifedOn);
            column.Properties.Add("DataType", "System.DateTime");
            column.Properties.Add("Nullable", false);
            defaultCols.Add(column);

            //add deleted on standard colum
            column = new DataEntity("DataPropertyDefinition");
            column.Properties.Add("Name", Globals.ScribeDeletedOn);
            column.Properties.Add("DataType", "System.DateTime");
            column.Properties.Add("Nullable", true);
            defaultCols.Add(column);

            return defaultCols;
        }



        /// <summary>
        /// This method is for comparing column properties 
        /// </summary>
        /// <param name="newColumns">Columns in question from the MethodInput</param>
        /// <param name="oldColumns">Existing columns</param>
        /// <returns></returns>
        private bool CheckForSchemaChanges(List<DataEntity> newColumns, List<DataEntity> oldColumns)
        {
            bool changeDetected = false;

            //the first check is to make sure that both tables
            //contain an equal number of columns
            if (newColumns.Count == oldColumns.Count)
            {
                //parse through the list of exisitng columns
                foreach (DataEntity oldColumn in oldColumns)
                {
                    bool columnFound = false;

                    //parse through the columns in questions until the existing one is found
                    foreach (DataEntity newColumn in newColumns)
                    {
                        string oldColumnName = GetPropertyValue("Name", oldColumn.Properties);
                        string oldDataType = DataTypeConverter.SystemToOleDb(GetPropertyValue("DataType", oldColumn.Properties));
                        string newColumnName = GetPropertyValue("Name", newColumn.Properties);
                        string newDataType = DataTypeConverter.SystemToOleDb(GetPropertyValue("DataType", newColumn.Properties));

                        //compare that the name of the column and the datatypes match
                        //otherwise a change has been made
                        if (oldColumnName == newColumnName && oldDataType == newDataType)
                        {
                            columnFound = true;
                            break;
                        }

                    }

                    //check if the existing column is found
                    if (columnFound == false)
                    {
                        changeDetected = true;
                        break;
                    }
                }
            }
            else
            {
                changeDetected = true;
            }

            return changeDetected;
        }

        #endregion
    }
}
