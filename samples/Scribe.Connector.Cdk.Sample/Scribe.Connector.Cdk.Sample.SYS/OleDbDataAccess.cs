// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OleDbDataAccess.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Core.ConnectorApi.Query;

namespace Scribe.Connector.Cdk.Sample.SYS
{
    /// <summary>
    /// Data access class which uses the OleDb type of data connection
    /// </summary>
    class OleDbDataAccess
    {
        #region member variables

        private OleDbConnection _connection = new OleDbConnection();

        /// <summary>
        /// Current database connection
        /// </summary>
        public OleDbConnection DbConnection
        {
            get { return _connection; }
        }

        /// <summary>
        /// Flag that will define whether the connector currently 
        /// has an open connection to the selected datasource
        /// </summary>
        public bool IsConnected;
        #endregion

        #region public connection methods
        /// <summary>
        /// Initialize a new connection using the OleDbConnection
        /// </summary>
        /// <param name="connectionString">Connection String to pass in</param>
        public void OleDbConnect(string connectionString)
        {
            //set the database connection parameters
            _connection.ConnectionString = connectionString;
            //attempt a connection to the database
            _connection.Open();

            //once connected set the IsConnected flag
            IsConnected = true;
        }

        /// <summary>
        /// Method to disconnect from the third party connection
        /// </summary>
        public void OleDbDisconnect()
        {
            //check the IsConnected flag prior to attempting a disconnect
            if (IsConnected)
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    //Attempt to close the open connection
                    _connection.Close();
                }
                //set the IsConnected flag
                IsConnected = false;
            }
        }
        #endregion

        #region Data Access Methods
        /// <summary>
        /// Execute a standard query through the Oledb connection
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public DataTable Execute(string query)
        {
            DataTable table = new DataTable();

            //check if there is an open connection
            if (IsConnected)
            {
                OleDbCommand command = new OleDbCommand(query, _connection);
                OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                adapter.Fill(table);
            }

            return table;
        }

        /// <summary>
        /// Execute the query provided
        /// </summary>
        /// <param name="tableName">name of the parent table that the query is being executed for</param>
        /// <param name="query">query to execute</param>
        /// <returns>enumerated list of data entities</returns>
        public IEnumerable<DataEntity> Execute(string tableName, string query)
        {
            return Execute(tableName, query, null);
        }

        /// <summary>
        /// Execute the query provided
        /// </summary>
        /// <param name="tableName">name of the parent table that the query is being executed for</param>
        /// <param name="query">query to execute</param>
        /// <param name="foreignKeyRelations">list of relations and foreign key values</param>
        /// <returns>enumerated list of data entities</returns>
        public IEnumerable<DataEntity> Execute(string tableName, string query, Dictionary<string, string> foreignKeyRelations)
        {
            //check if there is an open connection
            if (IsConnected)
            {
                //create the command to request data
                OleDbCommand oleDbCommand = new OleDbCommand();
                oleDbCommand.CommandText = query;
                oleDbCommand.Connection = _connection;

                //execute the reader
                OleDbDataReader dataReader = oleDbCommand.ExecuteReader();

                using (dataReader)
                {
                    //check that the reader is not null and that rows were returned
                    if (dataReader != null && dataReader.HasRows)
                    {
                        int fieldsReturned = dataReader.FieldCount;

                        //perform reading of data while get rows from the reader
                        while (dataReader.Read())
                        {
                            //create a new data entity for each row that is being read
                            DataEntity dataEntity = new DataEntity(tableName);

                            //parse through each field in the row
                            for (int i = 0; i < fieldsReturned; i++)
                            {
                                if (ParseRelationships(dataEntity, i, dataReader))
                                {
                                    continue;
                                }

                                //get the name of the column
                                var columnName = dataReader.GetName(i);
                                //get the value of the column in its correct datatype
                                var dataValue = dataReader.GetProviderSpecificValue(i);
                                //convert DB Null values to standard null values
                                if (dataValue == DBNull.Value)
                                {
                                    dataValue = null;
                                }
                                //add the column information to the data enitity
                                dataEntity.Properties.Add(columnName, dataValue);

                            }


                            ValidateEntityRelations(dataEntity, foreignKeyRelations);

                            //yield the data entity, this allows for data to be processed while more data is being retrieved
                            yield return dataEntity;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Execute the non query command
        /// </summary>
        /// <param name="query"></param>
        public int ExecuteNonQuery(string query)
        {
            int rowsEffected = 0;
            //check the IsConnected flag prior to attempting to execute the query
            if (IsConnected)
            {
                Logger.Write(Logger.Severity.Debug, "Executed Query", query);
                OleDbCommand command = new OleDbCommand(query, _connection);
                rowsEffected = command.ExecuteNonQuery();
            }

            return rowsEffected;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// This method is used to parse relationship data returned from the 
        /// database and places it in a hierarchial model
        /// </summary>
        /// <param name="dataEntity">data entity that is currently being filled with data returned from the database</param>
        /// <param name="index">index of the datareader</param>
        /// <param name="dataReader">datareader that contains the results of the query</param>
        /// <returns>true if the data is in relationship form, false if it is the root entity</returns>
        private bool ParseRelationships(DataEntity dataEntity, int index, IDataReader dataReader)
        {
            //holds the value to indicate if this is the root entity or a realtionship entity
            bool isRelationshipValue = false;
            //pulls apart and alias for a column, ex: [RelationshipName].[TableName].[ColumnName]
            var relationshipHierarchy = dataReader.GetName(index).Split('.');

            //check if this is the root entity which will only return the column name
            if (relationshipHierarchy.Length > 1)
            {
                isRelationshipValue = true;
                //pull apart the relationship information
                string relationshipName = relationshipHierarchy[0];
                string tableName = relationshipHierarchy[1];
                string columnName = relationshipHierarchy[2];
                //retrieve the data for the relationship
                object data = dataReader.GetValue(index);
                //check for null values
                if (data == DBNull.Value)
                {
                    data = null;
                }
                //check if the realtionship exists, create it if it doesn't
                if (dataEntity.Children.ContainsKey(relationshipName) == false)
                {
                    DataEntity parentEntity = new DataEntity();
                    parentEntity.ObjectDefinitionFullName = tableName;
                    parentEntity.Properties.Add(relationshipHierarchy[2], data);
                    dataEntity.Children.Add(relationshipHierarchy[0], new List<DataEntity>() { parentEntity });
                }
                else
                {
                    List<DataEntity> relatedEntities = dataEntity.Children[relationshipName];
                    //if the realtionship exists then do further validation
                    AppendRelationshipData(relatedEntities, columnName, tableName, data);
                }
            }

            return isRelationshipValue;
        }

        /// <summary>
        /// Adds the data retrieved from the database to an existing list of relationship entities
        /// </summary>
        /// <param name="relatedEntities">current list of related entities</param>
        /// <param name="columnName">name of the column</param>
        /// <param name="tableName">name if the table</param>
        /// <param name="data">data retrieve as the result of the query for this column</param>
        private void AppendRelationshipData(List<DataEntity> relatedEntities, string columnName, string tableName, object data)
        {
            //indicates whether a new related entity needs to be created
            bool createNewDataEntity = false;

            //check each entity in the list to see if it already contains the column
            foreach (var relatedEntity in relatedEntities)
            {
                //check if the column exists
                if (relatedEntity.Properties.ContainsKey(columnName))
                {
                    createNewDataEntity = true;
                }
                else
                {
                    //add the column if it does not already exist and fill it with data
                    relatedEntity.Properties.Add(columnName, data);
                    createNewDataEntity = false;
                    break;
                }
            }

            //add a new data entity if the column already exists in all current entities
            if (createNewDataEntity)
            {
                DataEntity parentEntity = new DataEntity();
                parentEntity.ObjectDefinitionFullName = tableName;
                parentEntity.Properties.Add(columnName, data);
                relatedEntities.Add(parentEntity);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataEntity"></param>
        /// <param name="foreignKeyRelations"></param>
        private void ValidateEntityRelations(DataEntity dataEntity, Dictionary<string, string> foreignKeyRelations)
        {
            //verify that the query processed contained requested relationship data
            if (foreignKeyRelations != null && dataEntity.Children != null)
            {
                //loop through each relationship for the current entity
                foreach (var entityChild in dataEntity.Children)
                {
                    //Continue IF....
                    //1: The list of foreign key relationships does not contain the specified relationship
                    //2: The list of entities are null for the relationship
                    //3: There are no related entities for the relationship
                    if (!foreignKeyRelations.ContainsKey(entityChild.Key) || entityChild.Value == null || entityChild.Value.Count <= 0)
                    {
                        continue;
                    }

                    //split the comma seperated list of foreign keys
                    var relationKeys = foreignKeyRelations[entityChild.Key].Split(',');
                    //set the child entity to the first entity child in the list.
                    //Note: since this is a child-to-parent relationship there will only be one value for each of the related entity lists
                    DataEntity childEntity = entityChild.Value[0];
                    
                    //validate that the related entites contain relationship data
                    if(CheckForNullChildEntities(entityChild.Value[0], relationKeys))
                    {
                        entityChild.Value[0] = null;
                    }

                }

                //clean up entity relations
                CleanDataEntityRelations(dataEntity);
            }
        }

        /// <summary>
        /// Validate that the child entity contains values for the foreign keys,
        /// If they are null then there was no value returned from the database therefore this child entity may be removed
        /// </summary>
        /// <param name="childEntity">child entitiy being processed</param>
        /// <param name="relationshipKeys">list of foreign keys for the relationship</param>
        /// <returns>True if the child entity that was processed should be a removed from the result set</returns>
        private bool CheckForNullChildEntities(DataEntity childEntity, string[] relationshipKeys)
        {
            bool removeEntity = false;
            //loop through each of the foreign keys in the relationship
            foreach (var relationKey in relationshipKeys)
            {
                //check if the foreign key exists in the entity properties and if the value is not null
                if (childEntity.Properties.ContainsKey(relationKey) && childEntity.Properties[relationKey] != null)
                {
                    removeEntity = false;
                    break;
                }

                removeEntity = true;
            }

            return removeEntity;
        }

        /// <summary>
        /// Remove an entity relationshions that are an empty list or null values
        /// </summary>
        /// <param name="dataEntity"></param>
        private void CleanDataEntityRelations(DataEntity dataEntity)
        {
            //create a tempory list of entity children to place the cleaned data into
            EntityChildren entityChildren = new EntityChildren();

            //loop through each child relationship
            foreach (var entityRelation in dataEntity.Children)
            {
                if (entityRelation.Value != null)
                {
                    //retrieve a list of data entities that are not a null value 
                    //(note: there should only be one for child-to-parent relations)
                    List<DataEntity> relations = entityRelation.Value.Where(entityChild => entityChild != null).ToList();
                    if (relations.Count > 0)
                    {
                        //add the list of relations to the entity children
                        entityChildren.Add(entityRelation.Key, relations);
                    }
                }
            }

            //add the child relations to the data entity
            dataEntity.Children = entityChildren;
        }

        #endregion
    }
}
