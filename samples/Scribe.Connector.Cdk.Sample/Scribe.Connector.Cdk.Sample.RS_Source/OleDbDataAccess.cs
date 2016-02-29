// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OleDbDataAccess.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using Scribe.Core.ConnectorApi;

namespace Scribe.Connector.Cdk.Sample.RS_Source
{
    using System.Data.OleDb;

    /// <summary>
    /// Data Access Class for the Ole Db connection
    /// </summary>
    public class OleDbDataAccess
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
                //Attempt to close the open connection
                _connection.Close();
                //set the IsConnected flag
                IsConnected = false;
            }
        }
        #endregion

        #region public data methods

        /// <summary>
        /// Get data for replication since the last time it was modified
        /// </summary>
        /// <param name="tableName">Name of the table to get the replication data from</param>
        /// <param name="lastSyncDate">Value of the last time the data was syncronized</param>
        /// <param name="hasLastModifiedColumn">identifies whether of not the object contains the last modifed column</param>
        /// <returns></returns>
        public IEnumerable<DataEntity> GetReplicationDataRetrieve(string tableName, DateTime lastSyncDate, bool hasLastModifiedColumn)
        {
            //create the select query
            string query = "SELECT * FROM [" + tableName + "]";

            //check if the last modified column name is specified
            if (hasLastModifiedColumn)
            {
                //round the milliseconds to the nearest second
                if (lastSyncDate.Millisecond > 0)
                {
                  lastSyncDate = lastSyncDate.AddSeconds(1);
                }

                if (lastSyncDate.Kind != DateTimeKind.Utc)
                {
                    lastSyncDate = lastSyncDate.ToUniversalTime();
                }

                query += " WHERE [ModifiedOn] > convert(datetime, '" + lastSyncDate.ToString("s") + "')";
            }

            //check if there is an open connection
            if (IsConnected)
            {
                //create the command to request data
                OleDbCommand oleDbCommand = new OleDbCommand();
                oleDbCommand.CommandText = query;
                oleDbCommand.Connection = _connection;

                //execute the reader
                OleDbDataReader dataReader = oleDbCommand.ExecuteReader();

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
                            //get the name of the column
                            var columnName = dataReader.GetName(i);
                            //get the value of the column in its correct datatype
                            var dataValue = dataReader.GetProviderSpecificValue(i);
                            //add the column information to the data enitity
                            dataEntity.Properties.Add(columnName, dataValue);
                        }
                        //yield the data entity, this allows for data to be processed while more data is being retrieved
                        yield return dataEntity;
                    }
                }
            }
        }

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
        /// Simply ExecuteNonQuery using the open connection
        /// </summary>
        /// <param name="query">string representation of the query to execute</param>
        public int ExecuteNonQuery(string query)
        {
            int recordsChanged = 0;

            //check if there is an open connection
            if (IsConnected)
            {
                OleDbCommand command = new OleDbCommand(query, _connection);
                recordsChanged = command.ExecuteNonQuery();
            }

            return recordsChanged;
        }
        #endregion

    }
}
