// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetadataProvider.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Core.ConnectorApi.Metadata;

namespace Scribe.Connector.Cdk.Sample.RS_Source
{
    /// <summary>
    /// This is the connectors implementation of the Scribe IMetadataProvider interface
    /// </summary>
    class MetadataProvider : IMetadataProvider
    {
        #region constants
        public const string MethodReturnName = "Result";
        #endregion

        #region member variables
        private readonly OleDbMetadataAccess _metadataAccess;
        #endregion

        #region ctor
        public MetadataProvider(OleDbMetadataAccess metadataAccess)
        {
            _metadataAccess = metadataAccess;
        }
        #endregion

        #region IMetadataProvider Implementation
        public void Dispose()
        {
            return;
        }

        /// <summary>
        /// There is no need to reset meta data here 
        /// since this connector retrieves meta data when it is requested rather than caching it
        /// </summary>
        public void ResetMetadata()
        {
            return;
        }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            throw new NotImplementedException();
        }

        public IMethodDefinition RetrieveMethodDefinition(string objectName, bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve a specific definition for an object including all property definitions
        /// TODO: Add examples for GetObjectProperties, and GetPropertyRelations
        /// </summary>
        /// <param name="objectName">Name of the object to retrieve the definition for</param>
        /// <param name="shouldGetProperties">Defines whether or not to get a list of properties associated with each object</param>
        /// <param name="shouldGetRelations">Defines whether or not to get a list of relations with each object such as foreign keys</param>
        /// <returns>Definition of the requested object</returns>
        public IObjectDefinition RetrieveObjectDefinition(string objectName, 
            bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            IObjectDefinition objectDefinition = null;

            //log the trace for the method execution
            using (new LogMethodExecution(
                Globals.ConnectorName, "GetObjectDefinition"))
            {
                //retrieve the table's index information
                DataTable tableIndexDefinition = 
                    _metadataAccess.GetTableIndexInformation(objectName);
                //retrieve the table's column information
                DataTable columnDefinitions = 
                    _metadataAccess.GetColumnDefinitions(objectName);
                //set the object definition
                objectDefinition = 
                    SetObjectDefinition(tableIndexDefinition, columnDefinitions);
            }

            return objectDefinition;
        }

        /// <summary>
        /// Retrieve a list of Object Definitions
        /// TODO: Add examples for GetObjectProperties, and GetPropertyRelations
        /// </summary>
        /// <param name="shouldGetProperties">Defines whether or not to get a list of properties associated with each object</param>
        /// <param name="shouldGetRelations">Defines whether or not to get a list of relations with each object such as foreign keys</param>
        /// <returns>List of Object Defintions</returns>
        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(
            bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            //create a new list of object definitions to return the list a tables in
            List<IObjectDefinition> objectDefinitions = new List<IObjectDefinition>();

            using (new LogMethodExecution(
                Globals.ConnectorName, "GetObjectList"))
            {
                //get the list of tables
                DataTable tableDefinitions = _metadataAccess.GetTableList();

                //add each table to the object definition list
                foreach (DataRow table in tableDefinitions.Rows)
                {
                    //create a new object defining the name of the table and 
                    //description using the information returned from metadata
                    var objectDefinition = new ObjectDefinition
                    {
                        Name = table["TABLE_NAME"].ToString(), 
                        FullName = table["TABLE_NAME"].ToString(),
                        Description = GetTableDescription(table["TABLE_NAME"].ToString())
                    };
                    //add the newly created object definition to the definition list
                    objectDefinitions.Add(objectDefinition);

                    //Set the hidden attribute to true if this is the change history table.
                    //Note: this is how to set items to not recommended for replication in the UI
                    objectDefinition.Hidden = objectDefinition.Name == Globals.ChangeHistoryTableName;
                }

            }
            //send back the object definition list 
            return objectDefinitions;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Set the defintion of the object using the 
        /// table information returned from the meta data
        /// </summary>
        /// <param name="tableIndexInformation">
        /// Table index information returned from the database schema
        /// </param>
        /// <param name="columnDefinitions">
        /// Table column definitions returned from the database schema
        /// </param>
        /// <returns>Object definition</returns>
        private IObjectDefinition SetObjectDefinition(
            DataTable tableIndexInformation, DataTable columnDefinitions)
        {
            //create a null instance of an object definition
            ObjectDefinition objectDefinition = null;
            
            //check that information is stored in the table information
            if ((tableIndexInformation != null 
                && tableIndexInformation.Rows.Count != 0) &&
                (columnDefinitions != null 
                && columnDefinitions.Rows.Count != 0))
            {
                string primaryKey = string.Empty;
                
                //create a new instance of the object definition
                objectDefinition = new ObjectDefinition { Description = string.Empty, PropertyDefinitions = new List<IPropertyDefinition>()};

                //parse through each of the table indexes and set the primary key value
                List<string> primaryKeys = 
                    (from DataRow tableIndex in tableIndexInformation.Rows 
                     where Convert.ToBoolean(tableIndex["PRIMARY_KEY"]) 
                     select tableIndex["COLUMN_NAME"].ToString()).ToList();

                //parse through the column information and set each property definition
                foreach (DataRow columnDefinition in columnDefinitions.Rows)
                {
                    //get the property defintion using the column information
                    var propertyDefinition = 
                        SetPropertyDefinition(columnDefinition, 
                        primaryKeys.Contains(
                        columnDefinition["COLUMN_NAME"].ToString()));

                    if (propertyDefinition == null) continue;

                    //check that the primary key was found and 
                    //set the object definition name values
                    if (objectDefinition.Name == null)
                    {
                        objectDefinition.Name = 
                            columnDefinition["TABLE_NAME"].ToString();

                        objectDefinition.FullName = 
                            columnDefinition["TABLE_NAME"].ToString();

                        objectDefinition.Description 
                            = GetTableDescription(columnDefinition["TABLE_NAME"].ToString());
                    }
                     
                    //add the property information to the object information
                    objectDefinition.PropertyDefinitions.Add(propertyDefinition);
                }

                //Set the hidden attribute to true if this is the change history table.
                //Note: this is how to set items to not recommended for replication in the UI
                objectDefinition.Hidden = objectDefinition.Name == Globals.ChangeHistoryTableName;
            }

            return objectDefinition;
        }

        /// <summary>
        /// Set the Property definition object using the 
        /// column information returned from the schema
        /// </summary>
        /// <param name="columnDefinition">
        /// Column Definition information returned from the schema
        /// </param>
        /// <param name="isPrimaryKey">
        /// Defines whether or not we are dealing with the primary key
        /// </param>
        /// <returns>Completed Property definition</returns>
        private IPropertyDefinition SetPropertyDefinition(
            DataRow columnDefinition, bool isPrimaryKey)
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

            //***********IMPORTANT STEP***************
            //Convert the Data Type returned from the ole db schema to a .Net Data Type
            propertyDefinition.PropertyType = 
                DataTypeConverter.OleDbToSystem(
                columnDefinition["DATA_TYPE"]).ToString();

            propertyDefinition.MaxOccurs = 1;
            propertyDefinition.MinOccurs = 1;
            propertyDefinition.IsPrimaryKey = isPrimaryKey;
            //set the required action to the inverse of the nullable property
            propertyDefinition.RequiredInActionInput = 
                !Convert.ToBoolean(columnDefinition["IS_NULLABLE"]);

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

        #endregion
    }
}
