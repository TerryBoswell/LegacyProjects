// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorCodes.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Scribe.Connector.Cdk.Sample.SYS
{
    /// <summary>
    /// This calls contains all codes to use in the event of an error
    /// </summary>
    public class ErrorCodes
    {
        #region Error Code Properties
        /// <summary>
        /// This is the numberic identifier for the error
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// This is the text description of the error to display in the log or to the user
        /// </summary>
        public string Description { get; set; }
        #endregion

        #region Error Code List

        /// <summary>
        /// Unable to connect to source error message
        /// </summary>
        public static ErrorCodes Connection
        {
            get
            {
                return new ErrorCodes { Description = "ERROR CONNECTING TO DATABASE", Number = (int)ErrorCode.Connection };
            }
        }

        /// <summary>
        /// This message is used when a table could not be found in the schema
        /// </summary>
        public static ErrorCodes ObjectNotFound
        {
            get
            {
                return new ErrorCodes { Description = "NO OBJECT FOUND WITH NAME: {0}", Number = (int)ErrorCode.ObjectNotFound };
            }
        }

        /// <summary>
        /// This message is used when there is an error retrieving the object definition list
        /// </summary>
        public static ErrorCodes GetObjectList
        {
            get
            {
                return new ErrorCodes { Description = "GET OBJECT DEFINITION LIST ERROR", Number = (int)ErrorCode.GetObjectList };
            }
        }

        /// <summary>
        /// This message is used when there is a request for a method that does not exist
        /// </summary>
        public static ErrorCodes UnknownMethod
        {
            get
            {
                return new ErrorCodes { Description = "UNKNOWN METHOD {0}", Number = (int)ErrorCode.UnknownMethod };
            }
        }

        /// <summary>
        /// This message is used when there is a request for an operation that does not exist
        /// </summary>
        public static ErrorCodes UnknownOperation
        {
            get
            {
                return new ErrorCodes { Description = "UNKNOWN OPERATION {0}", Number = (int)ErrorCode.UnknownOperation };
            }
        }

        /// <summary>
        /// This message is used when the schema returns no objects
        /// </summary>
        public static ErrorCodes NoObjectsFound
        {
            get
            {
                return new ErrorCodes { Description = "NO OBJECTS FOUND", Number = (int)ErrorCode.NoObjectsFound };
            }
        }

        /// <summary>
        /// This message is used if a date is invalid
        /// </summary>
        public static ErrorCodes InvalidDate
        {
            get
            {
                return new ErrorCodes { Description = "INVALID DATE REQUESTED", Number = (int)ErrorCode.InvalidDate };
            }
        }

        /// <summary>
        /// This message is used when there is an error retrieving data from the source
        /// </summary>
        public static ErrorCodes GetData
        {
            get
            {
                return new ErrorCodes { Description = "ERROR RETRIEVING REPLICATION DATA: {0}", Number = (int)ErrorCode.GetData };
            }
        }

        /// <summary>
        /// This is the message when the cause of the issue is unknown
        /// </summary>
        public static ErrorCodes NoRowsFound
        {
            get
            {
                return new ErrorCodes { Description = "No rows meet lookup the criteria", Number = (int)ErrorCode.NoRowsFound };
            }
        }

        /// <summary>
        /// This is the message to store in the Error Info if ExecuteMethod has failed
        /// </summary>
        public static ErrorCodes MethodError
        {
            get
            {
                return new ErrorCodes { Description = "An error has occurred while executing method: {0}", Number = (int)ErrorCode.MethodError };
            }
        }

        /// <summary>
        /// This is the message when the cause of the issue is unknown
        /// </summary>
        public static ErrorCodes Unknown
        {
            get
            {
                return new ErrorCodes { Description = "UNKNOWN CONNECTOR ERROR", Number = (int)ErrorCode.Unknown };
            }
        }

        /// <summary>
        /// This is the message that will be logged if a query either has no object name or has an empty list of properties to return
        /// </summary>
        public static ErrorCodes InvalidQueryObject
        {
            get
            {
                return new ErrorCodes { Description = "Invalid Query Object", Number = (int) ErrorCode.InvalidQueryObject };
            }
        }

        /// <summary>
        /// This is a generic message to precede a connector error
        /// </summary>
        public static ErrorCodes GenericConnectorError
        {
            get
            {
                return new ErrorCodes { Description = "The following error has occurred in the SYS connector:", Number = (int) ErrorCode.GenericError };
            }
        }

        /// <summary>
        /// This is a generic message to precede a connector warning
        /// </summary>
        public static ErrorCodes GenericConnectorWarning
        {
            get
            {
                return new ErrorCodes { Description = "The following warning has occurred in the SYS connector:", Number = (int)ErrorCode.GenericWarning };
            }
        }


        /// <summary>
        /// This is the message to throw when a column that is not part of the selected table is requested by the user
        /// </summary>
        public static ErrorCodes InvalidQueryColumn
        {
            get { return new ErrorCodes { Description = "Column: [{0}] is not associated with table: [{1}]", Number = (int)ErrorCode.InvalidQueryColumn }; }
        }

        /// <summary>
        /// This is the message to throw when a table that is not part of the selected database is requested by the user
        /// </summary>
        public static ErrorCodes InvalidQueryTable
        {
            get
            {
                return new ErrorCodes { Description = "Table [{0}] is not associated with the selected database.", Number = (int)ErrorCode.InvalidQueryTable };
            }
        }

        /// <summary>
        /// This is the message to throw when unsupported data types is requested by the user
        /// </summary>
        public static ErrorCodes InvalidQueryDataType
        {
            get
            {
                return new ErrorCodes { Description = "Data Type [{0}] is unsupport by connector: {1}", Number = (int)ErrorCode.InvalidQueryDataType };
            }
        }

        /// <summary>
        /// This is the message to display when a query was unable to be executed
        /// </summary>
        public static ErrorCodes ExecuteQueryFailed
        {
            get
            {
                return new ErrorCodes { Description = "The following query was unable to be processed: {0}", Number = (int)ErrorCode.ExecuteQueryFailed };
            }
        }

        /// <summary>
        /// This is the message to display if OperationInput.AllowsMultipleResults is set to false 
        /// but multiple rows were found during the ExecuteOperation method call
        /// </summary>
        public static ErrorCodes TooManyRowsReturned
        {
            get
            {
                return new ErrorCodes  { Description = "{0} rows meet the provided lookup critera, only one row is expected.", Number = (int) ErrorCode.TooManyRowsFound };
            }
        }

        /// <summary>
        /// This is the message to show when anything other that an equals value is used to reference a null value
        /// </summary>
        public static ErrorCodes NullOperatorNotValid
        {
            get { return new ErrorCodes {Description = "Invalid Operator [{0}] found in query when referencing a NULL value. ", Number = (int) ErrorCode.NullOperatorNotValid}; }
        }

        #endregion

        #region Error Code Enumeration
        /// <summary>
        /// This is a list of constant error codes used for identification purposes
        /// </summary>
        private enum ErrorCode
        {
            Connection = 1,
            ObjectNotFound = 2,
            UnknownMethod = 3,
            UnknownOperation = 4,
            GetObjectList = 5,
            NoObjectsFound = 6,
            InvalidDate = 7,
            GetData = 8,
            MethodError = 9,
            InvalidQueryObject = 10,
            GenericError = 11,
            GenericWarning = 12,
            InvalidQueryColumn = 13,
            InvalidQueryTable = 14,
            InvalidQueryDataType = 15,
            ExecuteQueryFailed = 16,
            NoRowsFound = 17,
            TooManyRowsFound = 18,
            NullOperatorNotValid = 19,
            Unknown = 20
        }
        #endregion
    }
}

