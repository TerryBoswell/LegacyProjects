// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OleDbDataAccess.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Data.OleDb;
using System.Data;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Query;

namespace Scribe.Connector.Cdk.Sample.RS_Target
{
    /// <summary>
    /// Data Access Class for the Ole Db connection
    /// </summary>
    public class OleDbDataAccess
    {
        #region member variables
        /// <summary>
        /// Local instance of the connection that can only be manipulated within the class
        /// </summary>
        private OleDbConnection _connection = new OleDbConnection();

        /// <summary>
        /// Public instance of the connection
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
                OleDbCommand command = new OleDbCommand(query, _connection);
                rowsEffected = command.ExecuteNonQuery();
            }

            return rowsEffected;
        }

        /// <summary>
        /// This method will be used for creating a replicated table
        /// </summary>
        /// <param name="tableName">Name of the table being created</param>
        /// <param name="columnList">Data Entity list filled with the 
        /// columns of the table to replicate including ScribeOnline default columns</param>
        public void CreateTable(string tableName, List<DataEntity> columnList)
        {
            //create the create table query
            string tableQuery = GenerateCreateTableQuery(tableName, columnList);
            //execute the query
            ExecuteNonQuery(tableQuery);
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
        #endregion

        #region private methods
        /// <summary>
        /// This method is used for creating the query to create a table for replication
        /// </summary>
        /// <param name="tableName">Name of the table being created</param>
        /// <param name="columnList">Data Entity list filled with the 
        /// columns of the table to replicate including ScribeOnline default columns</param>
        /// <returns>create table query</returns>
        private string GenerateCreateTableQuery(string tableName, List<DataEntity> columnList)
        {
            string query = string.Format("CREATE TABLE [{0}](", tableName);
            List<string> indexList = new List<string>();
            //parse through each column to add it to the create query
            foreach (DataEntity column in columnList)
            {
                string indexKey = string.Empty;
                string columnProperties = ParseColumnProperties(column.Properties, out indexKey);

                string addColumnText = string.Format("{0} ,", columnProperties);

                query += addColumnText;

                if (string.IsNullOrWhiteSpace(indexKey) == false)
                {
                    indexList.Add(indexKey);
                }
            }

            //set the SCRIBE_ID as the primary key constraint
            query += string.Format(" CONSTRAINT [PK_{0}] PRIMARY KEY NONCLUSTERED ([{1}]) ON [PRIMARY]", tableName, Globals.ScribePrimaryKey);

            if (indexList.Count > 0)
            {
                query += ", UNIQUE CLUSTERED(";
                foreach (string key in indexList)
                {
                    query += " [" + key + "],";
                }
                query = query.TrimEnd(',') + ")  ";
            }

            return query + ")ON [PRIMARY]";
        }

        /// <summary>
        /// This column is used to parse the options for a column
        /// such as Type and length
        /// </summary>
        /// <param name="properties">Properties of the column being created</param>
        /// <param name="indexKey">set this to a primary key name or to string.empty</param>
        /// <returns></returns>
        private string ParseColumnProperties(EntityProperties properties, out string indexKey)
        {
            string columnOptions;
            indexKey = string.Empty;
            //parse property values
            string columnName = ParsePropertyValue("Name", properties, string.Empty);
            string dataType = ParsePropertyValue("DataType", properties, "System.String");
            string convertedType = DataTypeConverter.SystemToOleDb(dataType);
            string maxLength = ParsePropertyValue("MaximumLength", properties, string.Empty);
            bool isPrimaryKey = Convert.ToBoolean(ParsePropertyValue("InPrimaryKey", properties, "False"));
            string additionalProperties = string.Empty;

            //check for special conditions based on the datatype
            if (convertedType == "nvarchar")
            {
                //add the max length for 
                if (Convert.ToInt32(maxLength) > 4000)
                {
                    maxLength = "max";
                }
                else
                {
                    maxLength = "255";
                }
                additionalProperties = string.Format(" ({0})", maxLength);
            }

            //create the SCRIBE_ID primary key value
            if (columnName == Globals.ScribePrimaryKey)
            {
                columnOptions = string.Format(" Identity ({0},{1}) NOT NULL",
                    ParsePropertyValue("IdentitySeed", properties, "1"),
                    ParsePropertyValue("IdentityIncrement", properties, "1"));
            }
            else if (isPrimaryKey)
            {
                indexKey = columnName;
                columnOptions = string.Format("{0} NULL", additionalProperties);
            }
            else
            {
                columnOptions = string.Format("{0} NULL", additionalProperties);
            }

            //set the complete value ofr column creation and convert the data type
            string columnText = string.Format("[{0}] [{1}]{2}", columnName, convertedType,
                                              columnOptions);

            return columnText;
        }

        /// <summary>
        /// Get the value of a specific property
        /// </summary>
        /// <param name="propertyName">Name of the property to look for</param>
        /// <param name="properties">list of properties of a column</param>
        /// <param name="defaultValue">default value to set if the property is not found</param>
        /// <returns>value of the property requested</returns>
        private string ParsePropertyValue(string propertyName, EntityProperties properties, string defaultValue)
        {
            string propertyValue = string.Empty;

            propertyValue = properties.ContainsKey(propertyName) == false ? defaultValue : properties[propertyName].ToString();

            return propertyValue;
        }

        #endregion
    }
}
