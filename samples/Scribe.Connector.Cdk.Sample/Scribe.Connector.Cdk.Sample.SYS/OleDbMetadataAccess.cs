// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OleDbMetadataAccess.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Scribe.Connector.Cdk.Sample.SYS
{
    using System.Data;
    using System.Data.OleDb;

    class OleDbMetadataAccess
    {
        private readonly OleDbDataAccess _dataAccess;
        private readonly OleDbConnection _oleDbConnection;

        /// <summary>
        /// Constructor for the metadata access object
        /// </summary>
        /// <param name="oleDbDataAccess">Current data access class with an open connection</param>
        public OleDbMetadataAccess(OleDbDataAccess oleDbDataAccess)
        {
            _dataAccess = oleDbDataAccess;
            _oleDbConnection = _dataAccess.DbConnection;
        }

        /// <summary>
        /// Get a list of tables without the columns form the connected datasource
        /// </summary>
        /// <returns>DataTable containing table information retrieved form the OleDb connection</returns>
        public DataTable GetTableList()
        {
            DataTable returnTable = new DataTable();

            //check if the connection is active
            if (_dataAccess.IsConnected)
            {
                //get the table information using the open connection
                returnTable = _oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                                                                   new object[] { null, null, null, "TABLE" });
            }

            return returnTable;
        }

        /// <summary>
        /// Get the column schema information
        /// </summary>
        /// <param name="tableName">Name of the table to retrieve the schema information for</param>
        /// <returns></returns>
        public DataTable GetColumnDefinitions(string tableName)
        {
            DataTable returnTable = null;
            //check if the connection is active
            if (_dataAccess.IsConnected)
            {
                //get the column information for the specified table
                returnTable = _oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                                                                   new object[] { null, null, tableName, null });
            }

            return returnTable;
        }

        /// <summary>
        /// Get The Expanded table information for a specific table
        /// </summary>
        /// <param name="tableName">name of the table to get the expanded definition of</param>
        /// <returns></returns>
        public DataTable GetTableIndexInformation(string tableName)
        {
            DataTable returnTable = null;
            //check if the connection is active
            if (_dataAccess.IsConnected)
            {

                //get the column information for the specified table
                returnTable = _oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes,
                                                                   new object[] { null, null, null, null, tableName });
            }

            return returnTable;
        }

        /// <summary>
        /// Get The Expanded table information for a specific table
        /// </summary>
        /// <param name="tableName">name of the table to get the expanded definition of</param>
        /// <returns></returns>
        public DataTable GetTableForeignKeyInformation(string tableName)
        {
            DataTable returnTable = null;
            //check if the connection is active
            if (_dataAccess.IsConnected)
            {

                //get the column information for the specified table
                returnTable = _oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys,
                                                                   new object[] { null, null, null, null, null, tableName });
            }

            return returnTable;
        }

        /// <summary>
        /// Retrieve the description for the specific table
        /// </summary>
        /// <returns></returns>
        public DataTable GetTableDescription()
        {
            DataTable table = new DataTable();

            //check if the connection is active
            if (_dataAccess.IsConnected)
            {
                //use the provided query for the description text
                OleDbCommand command = new OleDbCommand(Globals.SelectDescriptionQuery, _oleDbConnection);
                OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                adapter.Fill(table);
            }

            return table;
        }
    }
}
