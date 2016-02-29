// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorCodes.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Scribe.Connector.Cdk.Sample.RS_Source
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
                return new ErrorCodes { Description = "ERROR CONNECTING TO RS SOURCE", Number = (int)ErrorCode.Connection };
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
        /// This error is used when there is an issue retrieving while initializing replication
        /// </summary>
        public static ErrorCodes InitReplication
        {
            get
            {
                return new ErrorCodes { Description = "ERROR INITIALIZTING CHANGE HISTORY", Number = (int)ErrorCode.InitReplication };
            }
        }

        /// <summary>
        /// This is the message to store in the Error Info if ExecuteMethod has failed
        /// </summary>
        public static ErrorCodes MethodError
        {
            get
            {
                return new ErrorCodes { Description = "An error has occured while executing method: {0}", Number = (int) ErrorCode.MethodError };
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
            InitReplication = 9,
            MethodError = 10,
            Unknown = 11
        }
        #endregion
    }
}
