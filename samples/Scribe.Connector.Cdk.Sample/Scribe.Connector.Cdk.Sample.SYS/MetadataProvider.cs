// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetadataProvider.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2012 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Core.ConnectorApi.Metadata;

namespace Scribe.Connector.Cdk.Sample.SYS
{
    class MetadataProvider : IMetadataProvider
    {
        #region member variables
        private readonly OleDbMetadataAccess _metadataAccess;
        #endregion

        #region ctor
        public MetadataProvider(OleDbMetadataAccess metadataAccess)
        {
            _metadataAccess = metadataAccess;
        }
        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            return;
        }

        #endregion

        #region Implementation of IMetadataProvider

        /// <summary>
        /// Retrieve a list of global actions that this particular connector supports, 
        /// these actions will be reflected in the operations that will be executed from
        /// 'ExecuteOperation' method found in the IConnector implemented class.
        /// Note: Object level action support is defined in RetrieveObjectDefinition and RetrieveObjectDefinitions.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            //List of action definitions to return
            List<IActionDefinition> actionDefinitions = new List<IActionDefinition>();

            //Log the  trace execution
            using (new LogMethodExecution(Globals.ConnectorName, "RetrieveActionDefinitions"))
            {
                var supportedOperations = Enum.GetValues(typeof(Globals.OperationType)).Cast<Globals.OperationType>();

                foreach (Globals.OperationType action in supportedOperations)
                {
                    ActionDefinition actionDefinition = new ActionDefinition();

                    // Setting the bulk property allows the connector to support 
                    // batches of requests with a single call from the API, the connector 
                    // will need to loop through all inputs when the user ops to use bulk operations.
                    actionDefinition.SupportsBulk = true;

                    // Setting the supports input flag allows the use of a target operation 
                    actionDefinition.SupportsInput = true;

                    //Set the action properties based on the type of action being defined
                    //Note: These may change depending on Connector support
                    switch (action)
                    {
                        case Globals.OperationType.Create:
                            //This declares the known api action, by default it will be set to none
                            actionDefinition.KnownActionType = KnownActions.Create;
                            break;
                        case Globals.OperationType.Update:
                            //setting the lookup conditions property allows for the use of properties 
                            //with the match field filtering when a map is created
                            actionDefinition.SupportsLookupConditions = true;
                            //setting the multiple record operations property allows the connector
                            //to support changes to multiple records with a single request
                            actionDefinition.SupportsMultipleRecordOperations = true;
                            actionDefinition.KnownActionType = KnownActions.Update;
                            break;
                        case Globals.OperationType.Delete:
                            //setting the lookup conditions property allows for the use of properties 
                            //with the match field filtering when a map is created
                            actionDefinition.SupportsLookupConditions = true;
                            //setting the multiple record operations property allows the connector
                            //to support changes to multiple records with a single request
                            actionDefinition.SupportsMultipleRecordOperations = true;
                            actionDefinition.KnownActionType = KnownActions.Delete;
                            break;
                        case Globals.OperationType.Upsert:
                            //setting the lookup conditions property allows for the use of properties 
                            //with the match field filtering when a map is created
                            actionDefinition.SupportsLookupConditions = true;
                            //setting the multiple record operations property allows the connector
                            //to support changes to multiple records with a single request
                            actionDefinition.SupportsMultipleRecordOperations = true;
                            actionDefinition.KnownActionType = KnownActions.UpdateInsert;
                            break;
                        default:
                            continue;
                    }

                    //Set the default action defintion properties
                    actionDefinition.Name = action.ToString();
                    actionDefinition.FullName = action.ToString();
                    actionDefinition.Description = string.Empty;

                    //Add the action definition to the action definitions list
                    actionDefinitions.Add(actionDefinition);
                }

                //Add the default Query Action

                ActionDefinition queryActionDefinition = new ActionDefinition
                {
                    KnownActionType = KnownActions.Query,
                    FullName = KnownActionNames.Query,
                    Name = KnownActionNames.Query,
                    Description = string.Empty,
                    //this property allows for filtering of properties
                    SupportsConstraints = true,
                    //this property identifies the support for relationship queries
                    SupportsRelations = true,
                    //this property allows for the use of an 'Order By' in the query
                    SupportsSequences = true,
                    SupportsMultipleRecordOperations = true
                };

                actionDefinitions.Add(queryActionDefinition);

            }

            return actionDefinitions;
        }

        /// <summary>
        /// Retrieve a specific definition for an object including all property definitions
        /// </summary>
        /// <param name="objectName">Name of the object to retrieve the definition for</param>
        /// <param name="shouldGetProperties">Defines whether or not to get a list of properties associated with each object</param>
        /// <param name="shouldGetRelations">Defines whether or not to get a list of relations with each object such as foreign keys</param>
        /// <returns>Definition of the requested object</returns>
        public IObjectDefinition RetrieveObjectDefinition(string objectName, bool shouldGetProperties, bool shouldGetRelations)
        {
            ObjectDefinition objectDefinition = null;

            //log the trace for the method execution
            using (new LogMethodExecution(Globals.ConnectorName, "GetObjectDefinition"))
            {
                //get the list of tables
                DataTable tableDefinitions = _metadataAccess.GetTableList();

                //add each table to the object definition list
                foreach (DataRow table in tableDefinitions.Rows)
                {
                    if (table["TABLE_NAME"].ToString() != objectName)
                    {
                        continue;
                    }
                    //create a new object defining the name of the table and 
                    //description using the information returned from metadata
                    objectDefinition = new ObjectDefinition
                    {
                        Name = table["TABLE_NAME"].ToString(),
                        FullName = table["TABLE_NAME"].ToString(),
                        Description = GetTableDescription(table["TABLE_NAME"].ToString()),
                        RelationshipDefinitions = new List<IRelationshipDefinition>(),
                        PropertyDefinitions = new List<IPropertyDefinition>()
                    };

                    //Set the hidden attribute to true if this is the change history table.
                    //Note: this is how to set items to not recommended for replication in the UI
                    objectDefinition.Hidden = objectDefinition.Name == Globals.ChangeHistoryTableName;
                }
                if (objectDefinition != null)
                {
                    if (shouldGetProperties)
                    {
                        //retrieve the list of properties for this object
                        objectDefinition.PropertyDefinitions = GetTableProperties(objectDefinition.Name);
                    }

                    if (shouldGetRelations)
                    {
                        //retrieve the list of relationships for this object
                        objectDefinition.RelationshipDefinitions = GetTableRelations(objectDefinition.Name);
                    }

                    //Create a new list of supported actions and include the known query action if it is supported in the connector
                    objectDefinition.SupportedActionFullNames = new List<string> { KnownActionNames.Query };

                    //Add each of the operations defined in this connector to the list of supported actions
                    //Note: These will also need to be added to the global list of supported actions
                    //Note: Certain permissions may prevent some actions for the logged in user, this should be reflected here.
                    foreach (Globals.OperationType supportActionEnum in Enum.GetValues(typeof(Globals.OperationType)).Cast<Globals.OperationType>())
                    {
                        //do not add the None operation, this is for internal use only
                        if (supportActionEnum == Globals.OperationType.None)
                        {
                            continue;
                        }

                        objectDefinition.SupportedActionFullNames.Add(supportActionEnum.ToString());
                    }
                }
            }

            return objectDefinition;
        }

        /// <summary>
        /// Retrieve a list of Object Definitions
        /// </summary>
        /// <param name="shouldGetProperties">Defines whether or not to get a list of properties associated with each object</param>
        /// <param name="shouldGetRelations">Defines whether or not to get a list of relations with each object such as foreign keys</param>
        /// <returns>List of Object Defintions</returns>
        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(bool shouldGetProperties, bool shouldGetRelations)
        {
            //create a new list of object definitions to return the list a tables in
            List<IObjectDefinition> objectDefinitions = new List<IObjectDefinition>();

            using (new LogMethodExecution(Globals.ConnectorName, "GetObjectList"))
            {
                //get the list of tables
                DataTable tableDefinitions = _metadataAccess.GetTableList();

                //add each table to the object definition list
                foreach (DataRow table in tableDefinitions.Rows)
                {
                    //Flag to identify if the table has a primary key value.
                    //If no primary key is found this table will not support the Upsert action
                    bool hasPrimaryKey;

                    //create a new object defining the name of the table and 
                    //description using the information returned from metadata
                    var objectDefinition = new ObjectDefinition
                                               {
                                                   Name = table["TABLE_NAME"].ToString(),
                                                   FullName = table["TABLE_NAME"].ToString(),
                                                   Description = GetTableDescription(table["TABLE_NAME"].ToString()),
                                                   RelationshipDefinitions = new List<IRelationshipDefinition>(),
                                                   PropertyDefinitions = new List<IPropertyDefinition>()
                                               };



                    //Retrieve the list of properties for this object.
                    var propertyDefinitions = GetTableProperties(objectDefinition.Name);

                    //Set wheather or not a primary key exists, this determines upsert support on this table.
                    hasPrimaryKey = propertyDefinitions.Any(propertyDefinition => propertyDefinition.IsPrimaryKey);

                    //Check if the property definitions were requested.
                    if(shouldGetProperties)
                    {
                        objectDefinition.PropertyDefinitions = propertyDefinitions;
                    }

                    //Check if relationships were requested.
                    if (shouldGetRelations)
                    {
                        //Retrieve the list of relationships for this object.
                        objectDefinition.RelationshipDefinitions = GetTableRelations(objectDefinition.Name);
                    }

                    //Create a new list of supported actions and include the known query action if it is supported in the connector
                    objectDefinition.SupportedActionFullNames = new List<string> { KnownActionNames.Query };

                    //Add each of the operations defined in this connector to the list of supported actions
                    //Note: These will also need to be added to the global list of supported actions
                    //Note: Certain permissions may prevent some actions for the logged in user, this should be reflected here.
                    foreach (
                        Globals.OperationType supportActionEnum in
                            Enum.GetValues(typeof(Globals.OperationType)).Cast<Globals.OperationType>())
                    {
                        //do not add the None operation, this is for internal use only
                        if (supportActionEnum == Globals.OperationType.None)
                        {
                            continue;
                        }

                        // Do not support the upsert operation if the table does not have a primary key.
                        // Since this property is not set it will not be display as an option in the UI.
                        if (supportActionEnum == Globals.OperationType.Upsert && !hasPrimaryKey)
                        {
                            continue;
                        }

                        objectDefinition.SupportedActionFullNames.Add(supportActionEnum.ToString());
                    }

                    //add the newly created object definition to the definition list
                    objectDefinitions.Add(objectDefinition);

                }

            }

            //send back the object definition list 
            return objectDefinitions;
        }

        /// <summary>
        /// This method is reserved for future use.
        /// </summary>
        /// <param name="shouldGetParameters"></param>
        /// <returns></returns>
        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is reserved for future use.
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="shouldGetParameters"></param>
        /// <returns></returns>
        public IMethodDefinition RetrieveMethodDefinition(string objectName, bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// There is no need to reset meta data here 
        /// since this connector retrieves meta data when it is requested rather than caching it
        /// </summary>
        public void ResetMetadata()
        {
            return;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Retrieves a list of column definitions for the specified table name.
        /// </summary>
        /// <param name="tableName">Name of the table to retrieve the properties for</param>
        /// <returns></returns>
        private List<IPropertyDefinition> GetTableProperties(string tableName)
        {
            List<IPropertyDefinition> properties = new List<IPropertyDefinition>();

            //retrieve the table's index information
            DataTable tableIndexDefinition =
                _metadataAccess.GetTableIndexInformation(tableName);

            //retrieve the table's column information
            DataTable columnDefinitions =
                _metadataAccess.GetColumnDefinitions(tableName);

            //parse through each of the table indexes and set the primary key value
            List<string> primaryKeys =
                (from DataRow tableIndex in tableIndexDefinition.Rows
                 where Convert.ToBoolean(tableIndex["PRIMARY_KEY"])
                 select tableIndex["COLUMN_NAME"].ToString()).ToList();

            //parse through the column information and set each property definition
            foreach (DataRow columnDefinition in columnDefinitions.Rows)
            {
                //get the property defintion using the column information
                var propertyDefinition = SetPropertyDefinition(columnDefinition, tableIndexDefinition,
                                          primaryKeys.Contains(
                                              columnDefinition["COLUMN_NAME"].ToString()));
                properties.Add(propertyDefinition);
            }

            return properties;
        }

        /// <summary>
        /// This method retrieves a list of parent child relationships for this particual object.
        /// </summary>
        /// <param name="tableName">Name of the table to retrieve the definitions for</param>
        /// <returns></returns>
        private List<IRelationshipDefinition> GetTableRelations(string tableName)
        {
            List<IRelationshipDefinition> relationshipDefinitions = new List<IRelationshipDefinition>();
            //Use meta data access to retrieve the foreign key information
            DataTable foreignKeyIndexes = _metadataAccess.GetTableForeignKeyInformation(tableName);

            //Parse through each relationship returned in the data table
            foreach (DataRow foreignKeyRow in foreignKeyIndexes.Rows)
            {
                if (relationshipDefinitions.Count > 0)
                {
                    bool keyFound = false;
                    //Check if the key has already been added to the list of relations.
                    foreach (var definition in relationshipDefinitions)
                    {
                        if (definition.Name != foreignKeyRow["FK_NAME"].ToString()) continue;
                        //Append the additional properties to the relationship
                        //Note: these must be added as a comma seperated list
                        definition.ThisProperties += "," + foreignKeyRow["FK_COLUMN_NAME"];
                        definition.RelatedProperties += "," + foreignKeyRow["PK_COLUMN_NAME"];
                        keyFound = true;
                        break;
                    }
                    //Don't create a new definition if the current one has been found
                    if (keyFound)
                    {
                        continue;
                    }
                }

                //Create a new RelationshipDefinition using the Foreign Key values
                RelationshipDefinition relationshipDefinition = new RelationshipDefinition
                {
                    Description = string.Empty,
                    Name = foreignKeyRow["FK_NAME"].ToString(),
                    FullName = foreignKeyRow["FK_NAME"].ToString(),
                    //This is the name of the Parent Object
                    ThisObjectDefinitionFullName = foreignKeyRow["FK_TABLE_NAME"].ToString(),
                    //This is the name of the field or fields in the Parent Object.
                    //Note: Multiple values must be a comma seperated list
                    ThisProperties = foreignKeyRow["FK_COLUMN_NAME"].ToString(),
                    //This is the name of the Referenced Object
                    RelatedObjectDefinitionFullName = foreignKeyRow["PK_TABLE_NAME"].ToString(),
                    //This is the name of the field or fields in the Referenced Object.
                    //Note: Multiple values must be a comma seperated list
                    RelatedProperties = foreignKeyRow["PK_COLUMN_NAME"].ToString(),
                };

                relationshipDefinitions.Add(relationshipDefinition);
            }


            return relationshipDefinitions;
        }

        /// <summary>
        /// Set the Property definition object using the 
        /// column information returned from the schema
        /// </summary>
        /// <param name="columnDefinition">
        /// Column Definition information returned from the schema
        /// </param>
        /// <param name="foreignKeys"></param>
        /// <param name="isPrimaryKey">
        /// Defines whether or not we are dealing with the primary key
        /// </param>
        /// <returns>Completed Property definition</returns>
        private IPropertyDefinition SetPropertyDefinition(
            DataRow columnDefinition, DataTable foreignKeys, bool isPrimaryKey)
        {
            PropertyDefinition propertyDefinition = new PropertyDefinition();

            //set the name and full name to the column name
            propertyDefinition.Name =
                columnDefinition["COLUMN_NAME"].ToString();
            propertyDefinition.FullName =
                columnDefinition["COLUMN_NAME"].ToString();
            //set the nullable value
            propertyDefinition.Nullable =
                Convert.ToBoolean(columnDefinition["IS_NULLABLE"]);

            //set the max length of the data
            if (string.IsNullOrWhiteSpace(
                columnDefinition["CHARACTER_MAXIMUM_LENGTH"].ToString()) == false)
            {
                propertyDefinition.Size =
                    Convert.ToInt32(columnDefinition["CHARACTER_MAXIMUM_LENGTH"]);
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

            //***********IMPORTANT STEP***************
            //Convert the Data Type returned from the ole db schema to a .Net Data Type
            propertyDefinition.PropertyType =
                DataTypeConverter.OleDbToSystem(
                columnDefinition["DATA_TYPE"]).ToString();

            propertyDefinition.MaxOccurs = 1;
            propertyDefinition.MinOccurs = 1;
            propertyDefinition.IsPrimaryKey = isPrimaryKey;

            //set the required action if no default value is specified and the field is not nullable
            if (Convert.ToBoolean(columnDefinition["IS_NULLABLE"]) == false && Convert.ToBoolean(columnDefinition["COLUMN_HASDEFAULT"]) == false)
            {
                propertyDefinition.RequiredInActionInput = true;
            }
            else
            {
                propertyDefinition.RequiredInActionInput = false;
            }

            //set default properties for supported actions
            //Note: these may differ based on the connector and user accessability to data fields
            propertyDefinition.UsedInActionInput = true;
            propertyDefinition.UsedInQuerySelect = true;
            propertyDefinition.UsedInQueryConstraint = true;
            propertyDefinition.UsedInActionOutput = true;
            propertyDefinition.UsedInLookupCondition = true;
            propertyDefinition.UsedInQuerySequence = true;

            propertyDefinition.Description = SetPropertyDescription(columnDefinition, foreignKeys);

            return propertyDefinition;
        }

        /// <summary>
        /// Retrieve the Table Description for the current table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private string GetTableDescription(string tableName)
        {
            DataTable descriptionTable = _metadataAccess.GetTableDescription();
            string description = string.Empty;

            //parse through each row of the table to fidn the current table description
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

        /// <summary>
        /// Set the description field in the property definition
        /// </summary>
        /// <param name="columnMetadata"></param>
        /// <param name="foreignKeys"></param>
        /// <returns></returns>
        private string SetPropertyDescription(DataRow columnMetadata, DataTable foreignKeys)
        {
            var description = new StringBuilder();

            //add native sql data type
            description.Append("Native Data Type: " + DataTypeConverter.NumericOleDbToString(columnMetadata["DATA_TYPE"]));

            if (Convert.ToBoolean(columnMetadata["COLUMN_HASDEFAULT"]))
            {
                description.Append(Environment.NewLine);
                description.Append("Default value: " + columnMetadata["COLUMN_DEFAULT"]);
            }

            //check if the value is unique
            if (foreignKeys.Rows.Cast<DataRow>().Where(foreignKey => foreignKey["COLUMN_NAME"].ToString() == columnMetadata["COLUMN_NAME"].ToString()).Any(foreignKey => Convert.ToBoolean(foreignKey["UNIQUE"])))
            {
                description.Append(Environment.NewLine + "Value is Unique.");
            }

            return description.ToString();
        }

        private bool CheckForPrimaryKeyField(List<PropertyDefinition> propertyDefinitions)
        {
            return propertyDefinitions.Any(propertyDefinition => propertyDefinition.IsPrimaryKey);
        }

        #endregion
    }
}
