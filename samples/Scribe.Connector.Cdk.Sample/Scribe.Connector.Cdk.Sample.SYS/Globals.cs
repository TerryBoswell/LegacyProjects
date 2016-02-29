// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Globals.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Scribe.Connector.Cdk.Sample.SYS
{
    internal static class Globals
    {
        /// <summary>
        /// This is the name of the connector in use
        /// </summary>
        public const string ConnectorName = "SYS Connector Sample";

        /// <summary>
        /// Error message that will be throw if a default input property could not be found
        /// </summary>
        public const string InputPropertyNotFound = "Input property not found in properties list";

        /// <summary>
        /// This is the query to select the table descriptions
        /// </summary>
        public const string SelectDescriptionQuery =
            "SELECT * FROM INFORMATION_SCHEMA.TABLES LEFT JOIN sys.extended_properties ex2 ON ex2.major_id = Object_id(TABLE_NAME) AND ex2.name = 'Description' WHERE TABLE_TYPE = 'BASE TABLE';";

        /// <summary>
        /// This is the name of the table created by the connector during InitReplication to store deleted records.
        /// </summary>
        public const string ChangeHistoryTableName = "ScribeChangeHistory";

        #region Enumerations
        /// <summary>
        /// This enumeration defines the types used to 
        /// distinguish the type of operation being executed
        /// </summary>
        public enum OperationType
        {
            None,
            Create,
            Update,
            Delete,
            Upsert
        }

        public enum QueryType
        {
            Insert,
            Update,
            Upsert,
            Delete,
            Select,
            Count

        }
        #endregion
    }
}
