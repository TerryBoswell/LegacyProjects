// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodHandler.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Common;
using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Core.ConnectorApi.Query;
using Scribe.Core.ConnectorApi.Metadata;

namespace Scribe.Connector.Cdk.Sample.RS_Source
{
    using System.Collections.Generic;
    using System.Data;


    internal class MethodHandler
    {
        #region private members
        //stores the current instance of the data access layer to use the current connection to access the metadata
        private readonly OleDbDataAccess _dataAccess;
        //This is the name of the field that tracks row updates
        private const string LastModifiedFieldName = "ModifiedOn";
        //this is the default primary key column/lookup column for this particular database
        private const string PrimaryKeyFieldName = "RecordId";
        //this is the message to indicate an invalid property name in the method input
        private const string InputPropertyNotFound = "Input property not found in properties list";
        //This is the file name for the change history create script
        private const string ChangeHistoryFileName = "ScribeChangeHistory_Create.sql";
        //this is the file name for the trigger create script
        private const string TriggerFileName = "ScribeDelete_Trigger.sql";
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
        /// Retrieve all IDs for entities that have changed since last syncronization.
        /// </summary>
        /// <param name="methodInput"></param>
        /// <returns></returns>
        public MethodResult GetChangeHistoryData(MethodInput methodInput)
        {
            MethodResult result = null;

            using (new LogMethodExecution(Globals.ConnectorName, "GetChangeHistory"))
            {
                //Check for the last sync date property
                if (methodInput.Input.Properties.ContainsKey("LastSyncDate") == false)
                {
                    throw new ArgumentNullException("LastSyncDate", InputPropertyNotFound);
                }

                //Retrieve the last date of syncronization from the method input.
                DateTime lastSyncDate = GetLastSyncDate(methodInput.Input.Properties);
                
                //Retrieve the name of the table from the method input.
                string tableName = GetPropertyValueName(
                    "ObjectName", methodInput.Input.Properties);

                string query = string.Format(
                "SELECT Id FROM ScribeChangeHistory WHERE ModifiedOn > convert(datetime,'{0}') AND TableName ='{1}'",
                lastSyncDate.ToString("s"), tableName);

                //Execute the query.
                DataTable records = _dataAccess.Execute(query);

                result = new MethodResult();

                if (records != null && records.Rows.Count > 0)
                {
                    List<DataEntity> entityList = new List<DataEntity>();

                    //Parse each row and add the records to the entity properties.
                    foreach (DataRow row in records.Rows)
                    {
                        //Create a new entity for each ID returned.
                        DataEntity entity = new DataEntity {ObjectDefinitionFullName = tableName};
                        //This the key name MUST be 'ChangeHistoryId'.
                        //The value MUST be the primary key value of the row that has been deleted
                        entity.Properties.Add("ChangeHistoryId", row["Id"]);
                        //Add the entity to the list.
                        entityList.Add(entity);
                    }

                    //Set the result return the the created data entity.
                    result.Return = new DataEntity("ChangeHistoryData");
                    result.Return.Properties.Add("EntityData", entityList);
                }
                else
                {
                    //Even if no data is being returned 
                    //make sure that the return has a value.
                    result.Return = new DataEntity();
                    result.Return.Properties.Add("EntityData", null);
                }
                
                result.Success = true;
            }

            return result;
        }


        /// <summary>
        /// This method creates an object used to 
        /// track changes for future replications.
        /// In this case, a delete trigger is added to a specified table. 
        /// Note: If the data-source already has a process for tracking changes, this 
        ///       method will only need to return a positive success in the method result
        /// </summary>
        /// <param name="methodInput">Method input used for the replication object.
        /// This is the name of the table that we need to extract.
        /// methodInput.Input.properties["ObjectName"]
        /// </param>
        /// <returns></returns>
        public MethodResult InitReplicationObject(MethodInput methodInput)
        {
            MethodResult methodResult = null;

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point 
            // is written during garbage collection.
            using (new LogMethodExecution(
                Globals.ConnectorName, "InitReplication"))
            {
                //First ensure the change history table exists
                MethodResult initReplicationResult = InitReplication();

                //If the replication table already exist then 
                //our work here is done and no other action is required
                if (initReplicationResult.Success)
                {
                    string tableName =
                         GetPropertyValueName("EntityName", methodInput.Input.Properties);
                    string triggerName =
                        string.Format("{0}_Deleted_TRG", tableName);

                    if (CheckForTrigger(triggerName, tableName) == false)
                    {
                        //Use the ConnectorApi provided local data storage to retrieve 
                        //and read the sql scripts contents
                        LocalDataStorage localDataStorage = new LocalDataStorage();
                        string deleteTriggerString = 
                            localDataStorage.ReadData(TriggerFileName);

                        if (string.IsNullOrWhiteSpace(deleteTriggerString))
                        {
                            throw new InvalidExecuteOperationException(string.Format("Unable to locate file: {0}", TriggerFileName)); 
                        }

                        string query = string.Format(deleteTriggerString, tableName);
                        //Execute the query to create the change history table.
                        _dataAccess.ExecuteNonQuery(query);
                    }

                    //If there were no errors in processing then 
                    //just set the Success for the method result to true.
                    methodResult = new MethodResult { Success = true };
                }
                else
                {
                    methodResult = SetErrorMethodResult(ErrorCodes.InitReplication.Number,
                                                        ErrorCodes.InitReplication.Description);
                }
            }

            return methodResult;
        }

        /// <summary>
        /// This method defines the process for tracking changes to data.
        /// This particular example shows one way how this operation may work. 
        /// Note: A seperate method will create triggers for each table that will fill this table with deletions.
        /// Note: If the data-source already has a process for tracking changes, this 
        ///       method will only need to return a positive success in the method result
        /// </summary>
        /// <returns></returns>
        public MethodResult InitReplication()
        {
            MethodResult methodResult = null;

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point 
            // is written during garbage collection.
            using (new LogMethodExecution(
                Globals.ConnectorName, "InitReplication"))
            {
                //First retrieve the object definition of the 
                //default table that is used for replication.
                MethodInput objectDefinitionInput = new MethodInput();
                objectDefinitionInput.Input.Properties.Add(
                    "ObjectName", "ScribeChangeHistory");
                MethodResult objectDefinitionResult =
                    GetObjectDefinition(objectDefinitionInput);

                //If the replication table already exist then our work
                //here is done. No other action is required.
                if (objectDefinitionResult.Success == false)
                {
                    //Use the Sribe-provided local data storage 
                    //to retrieve and read the sql scripts contents.
                    LocalDataStorage localDataStorage = new LocalDataStorage();
                    string query =
                        localDataStorage.ReadData(ChangeHistoryFileName);
                    
                    //Throw an error message if the file was not found
                    if (string.IsNullOrWhiteSpace(query))
                    {
                        throw new InvalidExecuteOperationException(string.Format("Unable to locate file: {0}", ChangeHistoryFileName));
                    }

                    //Execute the query to create the change history table.
                    _dataAccess.ExecuteNonQuery(query);
                }

                //If there were no errors in processing, then 
                //set the Success for the method result to true.
                methodResult = new MethodResult { Success = true };
            }

            return methodResult;
        }

        /// <summary>
        /// Get a specific Object's definition, this includes any attributes and
        /// supporting object properties.
        /// In this case retrieve the table definition along with any columns and
        /// the definition of each.
        /// </summary>
        /// <param name="methodInput">Method Input which includes an 'ObjectName' 
        /// property to determine the object to retrieve the definition.</param>
        /// <returns>Method Result, which will either include error information or the 
        /// Object Definition of the 'ObjectName' specified in the 
        /// MethodInput properties.</returns>
        public MethodResult GetObjectDefinition(MethodInput methodInput)
        {
            //Create a new instance of the method result to fill with 
            //meta data information
            MethodResult result = null;

            //Create a new instance of the metadata access class and pass the
            //data access instance along with it
            OleDbMetadataAccess metadataAccess = new OleDbMetadataAccess(_dataAccess);

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point 
            // is written during garbage collection.
            using (new LogMethodExecution(
                Globals.ConnectorName, "GetObjectDefinition"))
            {
                //Get the name of the object in the input properties.
                string objectName = 
                    GetPropertyValueName("ObjectName", methodInput.Input.Properties);

                //Use the metadata access to get the 
                //definitions for each of the columns in the table.
                DataTable tableColumnDefinitions =
                    metadataAccess.GetColumnDefinitions(objectName);

                //Using the meta data access get the definition for the 
                //table indexes (primary and foreign keys)
                DataTable tableIndexDefinition =
                    metadataAccess.GetTableIndexInformation(objectName);

                //Check that both sets of data have been returned 
                //from the meta data access layer
                if ((tableColumnDefinitions != null
                    && tableColumnDefinitions.Rows.Count != 0) &&
                    (tableIndexDefinition != null
                    && tableIndexDefinition.Rows.Count != 0))
                {
                    //Create a new replication service object
                    RSObjectDefinition rsObjectDefinition = new RSObjectDefinition()
                    {
                        Name = objectName,
                        RSPropertyDefinitions = new List<RSPropertyDefinition>()
                    };

                    //If this is the change history table set the hidden attribute.
                    //Note: this is how to prevent an object from being replicated.
                    rsObjectDefinition.Hidden = objectName == Globals.ChangeHistoryTableName;

                    List<string> tablePrimaryKeys =
                        GetTablePrimaryKeys(rsObjectDefinition.Name, metadataAccess);

                    //Parse each column returned from the column definitions.                    
                    //For each column, add a new replication service property definition 
                    //to the newly created replication service object definition.
                    foreach (DataRow columnDefinition in tableColumnDefinitions.Rows)
                    {
                        //Process the column definition and set it to the 
                        //resplication service property definition.
                        RSPropertyDefinition rsPropertyDefinition =
                            ProcessColumnDefinition(columnDefinition);

                        //Check if this is the default last modified column and 
                        //set the object property.
                        if (rsPropertyDefinition.Name == LastModifiedFieldName)
                        {
                            rsObjectDefinition.ModificationDateFullName =
                                rsPropertyDefinition.Name;
                        }

                        //Check if the property is a primary key value.
                        rsPropertyDefinition.InPrimaryKey =
                            tablePrimaryKeys.Contains(rsPropertyDefinition.Name);

                        //Add the property definition to the object definition.
                        rsObjectDefinition.RSPropertyDefinitions.Add(rsPropertyDefinition);
                    }
                    
                    //Convert the replication service object definition to a Data Entity.
                    //Set the result return value to the 
                    //replication service object definition.
                    //Set the result Success to true.
                    result = new MethodResult 
                    { Success = true, Return = rsObjectDefinition.ToDataEntity() };
                }
                else
                {
                    //Set the proper error information in the method result in the 
                    //event of a null table or column definitions.
                    result = SetErrorMethodResult(
                        ErrorCodes.NoObjectsFound.Number, 
                        ErrorCodes.NoObjectsFound.Description);
                }

            }

            //Return the method result.
            return result;
        }

        /// <summary>
        /// Get the list of 'Object' names or in this case 
        /// table names from the data source 
        /// include the primary key or identifyer in the each of the objects
        /// </summary>
        /// <returns>MethodResult to be return in a readable state</returns>
        public MethodResult GetObjectDefinitionList()
        {
            //Create a new instance of the method result to fill with 
            //meta data information
            MethodResult result = new MethodResult();

            // Create a new instance of the metadata access class and pass 
            // the data access instance along with it
            OleDbMetadataAccess metadataAccess =
                new OleDbMetadataAccess(_dataAccess);

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point 
            // is written during garbage collection.
            using (new LogMethodExecution(
                Globals.ConnectorName, "GetObjectDefinitionList"))
            {
                //Get a list of indexes for each table
                DataTable tableList = metadataAccess.GetTableList();

                //check that valid data has been return from the schema
                if (tableList != null && tableList.Rows.Count != 0)
                {
                    //Create a list of generic Data Entities
                    //***This is a Key piece of the replication process***
                    //This is where the table list will be stored in a 
                    //generic fashion and return in the result.
                    List<DataEntity> dataEntityList = new List<DataEntity>();

                    //Parse the list of rows that contain the table 
                    //information and stuff them into generic data entities. 
                    //Add them to the list that will be returned in the result.
                    foreach (DataRow tableRow in tableList.Rows)
                    {
                        var tableName = tableRow["TABLE_NAME"].ToString();

                        var dataEntity = new DataEntity("Object");
                        dataEntity.Properties.Add("Name", tableName);
                        dataEntity.Properties.Add(
                            "PrimaryKeyName", PrimaryKeyFieldName);
                        dataEntity.Properties.Add(
                            "Description", GetTableDescription(tableName));
                        //Check if the table has the ModifiedOn column
                        dataEntity.Properties.Add(
                            "ModificationDateFullName",
                            CheckForLastModifiedColumnName(
                                tableName, metadataAccess)
                                ? LastModifiedFieldName : string.Empty);

                        dataEntity.Properties.Add("Hidden", tableName == Globals.ChangeHistoryTableName);

                        dataEntityList.Add(dataEntity);
                    }

                    //Set the success of the result to true since the 
                    //list of entities has been filled.
                    result.Success = true;

                    //Create a new instance of the return result set
                    //with the name of the returned items.
                    result.Return = new DataEntity("ObjectList");

                    //Add the entity list to the result.
                    result.Return.Properties.Add("Result", dataEntityList);
                }
                else
                {
                    //Set the proper error information in the event that 
                    //incorrect schema information is returned from the database.
                    result = SetErrorMethodResult(
                        ErrorCodes.GetObjectList.Number, ErrorCodes.GetObjectList.Description);
                }
            }

            //Return the method result containing the object definition list.
            return result;
        }

        /// <summary>
        /// This is the method to get data for replication 
        /// for an individual object. Rows will be returned as they are read.
        /// </summary>
        /// <param name="methodInput"></param>
        /// <returns></returns>
        public MethodResult GetReplicationData(MethodInput methodInput)
        {
            MethodResult result = null;

            using (new LogMethodExecution(
                Globals.ConnectorName, "GetReplicationData"))
            {
                //Get the name of the object in the input properties.
                string objectName = GetPropertyValueName(
                    "ObjectName",methodInput.Input.Properties);


                //Get the last sychronization date from the input properties
                DateTime lastSyncDate = GetLastSyncDate(methodInput.Input.Properties);

                OleDbMetadataAccess metadataAccess = new OleDbMetadataAccess(_dataAccess);
                bool hasLastModified = CheckForLastModifiedColumnName(objectName, metadataAccess);

                try
                {
                    //Passes control of the lower level method in the data access layer 
                    //to the calling method.This passes the IEnumerable object 
                    //(filled with data rows) out to the calling methodin conjunction 
                    //with the yield statement at the lower level, is looking for the 
                    //foreach loop else control is passed out to the calling method.  
                    //Here, the attempt to get an entity is performed (by calling the 
                    //empty foreach loop) in order to check for errors - else the top 
                    //level has control and exceptions get passed directly up.  Here we assume
                    //that no error on retrieving row 1 means we are safe to pass control 
                    //back to the calling method and skip the error checking 
                    //that is forced on the first entity. 

                    IEnumerable<DataEntity> replicationData =
                        _dataAccess.GetReplicationDataRetrieve(
                        objectName, lastSyncDate, hasLastModified);

                    //Force a check for errors. The previous method call 
                    //will not be performed until it is requested for use here.
                    foreach (var dataEntity in replicationData)
                    {
                        break;
                    }

                    //Create a new method result
                    result = new MethodResult();
                    //Indicate that the result is a success
                    result.Success = true;
                    //Set the result return to a new data entity
                    //which MUST be named "ReplicationQueryData"
                    result.Return = new DataEntity("ReplicationQueryData");

                    //Add the yielded replication data to the return properties in the result
                    //Note: the property name MUST be labeled as 'EntityData'
                    result.Return.Properties.Add("EntityData", replicationData);
                }
                catch (Exception exception)
                {
                    //Be sure to log any errors and add them to the 
                    //error information for the method result
                    Logger.Write(
                        Logger.Severity.Error, Globals.ConnectorName, exception.Message);
                    result = SetErrorMethodResult(
                        ErrorCodes.GetData.Number, ErrorCodes.GetData.Description);
                }

            }
            //Return the method result containing the replication data
            return result;
        }

        #endregion

        #region private methods
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
        /// Get the table name form the passed in method input
        /// </summary>
        /// <param name="propertyName">Name of the property key to find</param>
        /// <param name="properties">the parameter values</param>
        /// <returns>the name of the table to retrieve</returns>
        private string GetPropertyValueName(string propertyName, EntityProperties properties)
        {
            // validate that an object name has been set
            if (!properties.ContainsKey(propertyName))
            {
                throw new ArgumentNullException(propertyName, InputPropertyNotFound);
            }

            var objectName = properties[propertyName].ToString();

            return objectName;
        }

        /// <summary>
        /// Get the table name form the passed in method input
        /// </summary>
        /// <param name="properties">the parameter values</param>
        /// <returns>the name of the table to retrieve</returns>
        private string GetPluginName(EntityProperties properties)
        {
            // validate that an object name has been set
            if (!properties.ContainsKey("PluginName") || string.IsNullOrWhiteSpace(properties["PluginName"].ToString()))
            {
                throw new ArgumentNullException("PluginName", Globals.InputPropertyNotFound);
            }

            var objectName = properties["PluginName"].ToString();

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
        /// Get the last sync date from the passed in method input
        /// </summary>
        /// <param name="properties">>The parameters of the MethodInput.</param>
        /// <returns>The last sync datetime of the table</returns>
        private DateTime GetLastSyncDate(EntityProperties properties)
        {
            DateTime lastSyncDate = new DateTime();
            DateTime minTime = new DateTime(1753, 1, 1);
            // validate key exists and set its value
            lastSyncDate = !properties.ContainsKey("LastSyncDate") ? minTime : Convert.ToDateTime(properties["LastSyncDate"]);

            if (lastSyncDate < minTime)
            {
                lastSyncDate = minTime;
            }

            // log value
            Logger.Write(Logger.Severity.Debug, Globals.ConnectorName, string.Format("LastSyncDate: {0}{1}", lastSyncDate, Environment.NewLine));

            return lastSyncDate;
        }

        /// <summary>
        /// Retrieve the column name used for checking the last date of synchronization on the column
        /// </summary>
        /// <param name="properties">The parameters of the MethodInput.</param>
        /// <returns>string value of the column name</returns>
        private string GetLastModifiedColumnNameFromInput(EntityProperties properties)
        {
            string columnName = string.Empty;

            //check if the column name is specified by checking for the 'ModificationDateFullName' property
            if (properties.ContainsKey("ModificationDateFullName"))
            {
                //set teh column name to the property in the property list
                columnName = properties["ModificationDateFullName"].ToString();
            }

            return columnName;
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
        /// Parse through the selected tables columns and check if the modified on column exists
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="metadataAccess">current instance of the meta data access layer</param>
        /// <returns>true if the table has hte ModifiedOn column</returns>
        private bool CheckForLastModifiedColumnName(string tableName, OleDbMetadataAccess metadataAccess)
        {
            bool hasModifiedOn = false;

            //get the list of column definitions
            DataTable columnList = metadataAccess.GetColumnDefinitions(tableName);

            //parse through the list of column definitions to check if the ModifiedOn column exists
            foreach (DataRow columnDefinition in columnList.Rows)
            {
                if (columnDefinition["COLUMN_NAME"].ToString() == LastModifiedFieldName)
                {
                    hasModifiedOn = true;
                    break;
                }
            }

            return hasModifiedOn;
        }

        /// <summary>
        /// Check the table if a specific trigger exists
        /// </summary>
        /// <param name="triggerName">name of the trigger to check for</param>
        /// <param name="tableName">name of the table that the trigger is associated with</param>
        /// <returns>true if the trigger exists already</returns>
        private bool CheckForTrigger(string triggerName, string tableName)
        {
            DataTable triggerInformation = _dataAccess.Execute(string.Format(Globals.SelectTriggersQuery));
            return triggerInformation.Rows.Cast<DataRow>().Any(row => row["name"].ToString() == triggerName);
        }

        /// <summary>
        /// Retrieves the description of the table from the schema information
        /// </summary>
        /// <param name="tableName">Name of the table to retrieve the description for</param>
        /// <returns>full description of the table that is store in the schema information</returns>
        private string GetTableDescription(string tableName)
        {
            DataTable descriptionTable = _dataAccess.Execute(Globals.SelectDescriptionQuery);
            string description = string.Empty;

            //parse through each of the tables to fine the one that was specified
            foreach (DataRow row in descriptionTable.Rows)
            {
                if (row["TABLE_NAME"].ToString() == tableName)
                {
                    description = row["value"].ToString();
                    break;
                }
            }

            return description;
        }

        #endregion
    }
}
