// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Globals.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Scribe.Connector.Cdk.Sample.RS_Target
{
    internal static class Globals
    {
        #region Connector Definition
        //This is the name of the connector in use
        public const string ConnectorName = "RS Target Connector Sample";
        #endregion

        #region Error Messages
        /// <summary>
        /// This is the message to use if an property is not found in the method input properties
        /// </summary>
        public const string InputPropertyNotFound = "Input property not found in properties list";
        #endregion

        #region ScribeDefaultColumns
        /// <summary>
        /// Default Scribe ID column
        /// </summary>
        public const string ScribePrimaryKey = "SCRIBE_ID";

        /// <summary>
        /// Default Scribe Created On column
        /// </summary>
        public const string ScribeCreatedOn = "SCRIBE_CREATEDON";

        /// <summary>
        /// Default Scribe Modified On column
        /// </summary>
        public const string ScribeModifedOn = "SCRIBE_MODIFIEDON";

        /// <summary>
        /// Default Scribe Deleted On column
        /// </summary>
        public const string ScribeDeletedOn = "SCRIBE_DELETEDON";

        #endregion

        #region Enumerations
        /// <summary>
        /// This enumeration defines the types used to 
        /// distinguish the type of operation being executed
        /// </summary>
        public enum OperationType
        {
            Create,
            Update,
            Delete
        }
        #endregion
    }
}
