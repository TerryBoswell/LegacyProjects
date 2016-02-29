// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RS_TargetConnector.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Data.OleDb;
using Scribe.Core.ConnectorApi.ConnectionUI;
using Scribe.Core.ConnectorApi.Cryptography;
using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.Logger;

namespace Scribe.Connector.Cdk.Sample.RS_Target
{
    using System;
    using System.Collections.Generic;
    using Core.ConnectorApi;
    using Core.ConnectorApi.Actions;
    using Core.ConnectorApi.Query;

    [ScribeConnector(ConnectorTypeIdAsString, ConnectorTypeName, ConnectorTypeDescription, typeof(RS_TargetConnector),
        SettingsUITypeName,
        SettingsUIVersion, ConnectionUITypeName, ConnectionUIVersion, XapFileName, new string[] { "Scribe.RS.Target" })]
    public class RS_TargetConnector : IConnector
    {
        #region private members
        private string _connectionString;
        private OleDbDataAccess _dataAccess = new OleDbDataAccess();

        /// <summary>
        /// Decryption key for securly passing sensitive data to the connector
        /// Note: this must be that same one both sides
        /// </summary>
        private static string CryptoKey
        {
            get { return "1E093A29-FBA8-4534-9EBD-AA7310477A70"; }
        }

        #endregion

        #region Members used by core for reflection

        /// <summary>
        ///   This constant holds the setting representing the name of the UI type.
        /// </summary>
        internal const string SettingsUITypeName = "";

        /// <summary>
        ///   This constant holds the setting for ui version.
        /// </summary>
        internal const string SettingsUIVersion = "1.0";

        /// <summary>
        ///   This constant holds the name of the UI connection type.
        /// </summary>
        internal const string ConnectionUITypeName = "ScribeOnline.GenericConnectionUI";

        /// <summary>
        ///   This constant holds the version of the UI connection.
        /// </summary>
        internal const string ConnectionUIVersion = "1.0";

        /// <summary>
        ///   This constant holds the name of the xap file.
        /// </summary>
        internal const string XapFileName = "ScribeOnline";

        /// <summary>
        ///   This constant holds this Connector Type Id.
        /// </summary>
        internal const string ConnectorTypeIdAsString = "9158B81E-5381-4560-8E55-BB792A35D00B";

        /// <summary>
        ///   The name of the Connector Type.
        /// </summary>
        internal const string ConnectorTypeName = "Cdk RS Target Sample";

        /// <summary>
        ///   The description of the Connector Type.
        /// </summary>
        internal const string ConnectorTypeDescription = "Cdk RS Target Sample from Scribe.";

        /// <summary>
        /// Default metadata prefix
        /// </summary>
        internal const string MetadataPrefix = "CdkRSTargetSample";

        #endregion

        #region public properties
        /// <summary>
        ///   This constant holds this Connector Type Id. This is a GUID that is used to verify the Connector
        /// </summary>
        public Guid ConnectorTypeId
        {
            get { return Guid.Parse("9158B81E-5381-4560-8E55-BB792A35D00B"); }
        }

        /// <summary>
        /// Gets the value indicating whether the Connector is connected
        /// </summary>
        public bool IsConnected
        {
            get { return _dataAccess.IsConnected; }
        }
        #endregion

        #region public methods
        /// <summary>
        /// This method will attempt to connect to the third-party and retrieve properties
        /// such as organizations databases etc.
        /// </summary>
        /// <param name="properties">
        /// The collection of information required by this Connector to connect to the third-party.
        /// </param>
        public string PreConnect(IDictionary<string, string> properties)
        {
            return BuildUiFormDefinition();
        }

        /// <summary>
        /// This method will attempt to connect to the third-party.
        /// </summary>
        /// <param name="properties">
        /// The collection of information required by this Connector to connect to the third-party.
        /// </param>
        public void Connect(IDictionary<string, string> properties)
        {
            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point is written during garbage collection.
            using (new LogMethodExecution(ConnectorTypeName, "Connect"))
            {
                try
                {
                    //parse the incomming properties to ensure the proper parameters are set
                    BuildConnectionString(properties);

                    //attempt a connection to the selected datasource
                    _dataAccess.OleDbConnect(_connectionString);
                }
                catch (OleDbException oleDbException)
                {
                    string msg = ExceptionFormatter.BuildActionableException("Unable to Connect to datasource", string.Format("The following error occured in the {0}:", ConnectorTypeName),
                                                                             oleDbException);
                    //be sure to log any errors that occure while attempting to connect
                    Logger.Write(Logger.Severity.Error, ConnectorTypeName, msg);
                    //throw the InvalidConnectionException found in the ConnectorApi
                    throw new InvalidConnectionException(msg);
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    string msg = ExceptionFormatter.BuildActionableException("Unable to connect to datasource the provider information is invalid.", string.Format("The following error occured in the {0}:", ConnectorTypeName),
                                                         invalidOperationException);
                    //be sure to log an error that is due to an invalid provider
                    Logger.Write(Logger.Severity.Error, ConnectorTypeName, msg);
                    //throw the InvalidConnectionException found in the ConnectorApi
                    throw new InvalidConnectionException(msg);
                }
            }
        }

        /// <summary>
        /// This method will attempt to disconnect from the third-party.
        /// </summary>
        public void Disconnect()
        {
            using (new LogMethodExecution(Globals.ConnectorName, "Disconnect"))
            {
                //verify that a connection is even open prior to disconnecting
                if (IsConnected)
                {
                    try
                    {
                        //attempt to disconnect from the current datasource
                        _dataAccess.OleDbDisconnect();
                    }
                    catch (Exception exception)
                    {
                        //Be sure to log any errors that occured while attempting to disconnect
                        Logger.Write(Logger.Severity.Error, "Disconnect Error", exception.Message);
                    }
                }
            }
        }

        /// <summary>
        /// This method will execute a Method returning a result. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public MethodResult ExecuteMethod(MethodInput input)
        {
            MethodResult methodResult;

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point is written during garbage collection.
            using (new LogMethodExecution(Globals.ConnectorName, "Execute Method"))
            {
                //Construct a new instance of the method handler 
                //passing along the current instance of the data access object
                MethodHandler methodHandler = new MethodHandler(_dataAccess);

                try
                {
                    //Use the name stored in the method 
                    //input to determine the correct method to execute
                    switch (input.Name.ToLower())
                    {
                        case "getobjectdefinition":
                            methodResult = methodHandler.GetObjectDefinition(input);
                            break;
                        case "createorupdateobjectforreplication":
                            methodResult = methodHandler.CreateOrUpdateObjectForReplication(input);
                            break;
                        case "getlastreplicationsyncdate":
                            methodResult = methodHandler.GetLastReplicationSyncDate(input);
                            break;
                        default:
                            string message = string.Format(ErrorCodes.UnknownMethod.Description, input.Name);
                            throw new InvalidExecuteMethodException(ErrorCodes.UnknownMethod.Number, message);
                    }

                    LogMethodResult(methodResult);
                }
                //Here we throw the Fatal Error Exception which is used to notify upper layers that 
                //an error has occured in the Connector and will be unable to recover from it
                catch (FatalErrorException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    //Log any other exceptions that occur during method execution 
                    //and store them in the MethodResult.ErrorInfo
                    methodResult = new MethodResult {Success = false};
                    
                    //Create the error info using the exception message
                    methodResult.ErrorInfo = new ErrorResult
                    {
                        Description = exception.Message,
                        Detail = exception.StackTrace,
                        Number = ErrorCodes.MethodError.Number
                    };

                    LogMethodResult(methodResult);
                }


            }
            return methodResult;
        }

        /// <summary>
        /// This method will execute an operation returning a result. This method is also used in bulk operations.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public OperationResult ExecuteOperation(OperationInput input)
        {
            OperationResult operationResult;

            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point 
            // is written during garbage collection.
            using (new LogMethodExecution
                (Globals.ConnectorName, "Execute Operation"))
            {
                //Construct a new instance of the operation handler
                //passing along the current instance of the data access object
                OperationHandler operationHandler = new OperationHandler(_dataAccess);

                try
                {
                    // Use the name stored in the operation 
                    // input to determine the correct operation to execute
                    switch (input.Name.ToLower())
                    {
                        case "delete":
                            operationResult = operationHandler.DeleteOperation(input);
                            break;
                        case "create":
                            operationResult = operationHandler.CreateOperation(input);
                            break;
                        case "update":
                            operationResult = operationHandler.UpdateOperation(input);
                            break;
                        default:
                            // Throw an exception when an operation 
                            // that does not exist is requested
                            throw new InvalidExecuteOperationException(
                                ErrorCodes.UnknownOperation.Number, 
                                ErrorCodes.UnknownOperation.Description);
                    }
                    LogOperationResult(operationResult);
                }
                //Here we throw the Fatal Error Exception which is 
                //used to notify upper layers that an error has occured 
                //in the Connector and will be unable to recover from it
                catch (FatalErrorException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    //Log any other exceptions that occur in method execution and 
                    //throw the Invalid Execute Operation Exception 
                    string msg = string.Format("{0} {1}",
                        Globals.ConnectorName, exception.Message);
                    Logger.Write(Logger.Severity.Error,
                        Globals.ConnectorName, msg);
                    throw new InvalidExecuteOperationException(msg);
                }
            }

            return operationResult;
        }

        /// <summary>
        /// The Connector will perform the query and pass the results back in an 
        /// enumerable set of ResultEntities.  Each of which could be a set of objects
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<DataEntity> ExecuteQuery(Query query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// get the open instance of the metadata provider
        /// </summary>
        /// <returns>Currenct open instance of the MetadataProvider</returns>
        public IMetadataProvider GetMetadataProvider()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region private Methods
        /// <summary>
        /// Parse through the imported connection properties and generate a connection string out of it
        /// </summary>
        /// <param name="properties">Connection properties from the connect method</param>
        private void BuildConnectionString(IDictionary<string, string> properties)
        {
            //check for each of the OleDb connection properties
            var provider = properties.ContainsKey("Provider") ? properties["Provider"] : string.Empty;
            var server = properties.ContainsKey("Server") ? properties["Server"] : string.Empty;
            var dataBase = properties.ContainsKey("Database") ? properties["Database"] : string.Empty;
            var userId = properties.ContainsKey("UserName") ? properties["UserName"] : string.Empty;
            var password = properties.ContainsKey("Password") ? properties["Password"] : string.Empty;

            //decrypt the password using the built in decryption method and the key generated for the connector
            password = Decryptor.Decrypt_AesManaged(password, CryptoKey);

            //constuct the connection string for use in the final connection to the datasource
            _connectionString = string.Format("Provider={0};Server={1};Database={2};Uid={3};Pwd={4};",
                provider, server, dataBase, userId, password);
        }

        /// <summary>
        /// Construct an XML serialized form that will define the input properties that will be displayed in the UI and 
        /// used to pass back into the connection using the connect method the connection method
        /// </summary>
        /// <returns></returns>
        private string BuildUiFormDefinition()
        {
            FormDefinition formDefinition = new FormDefinition();
            //Add your company name here, this is a required field 
            formDefinition.CompanyName = "Scribe Software Corporation © 1996-2011 All rights reserved.";
            //declare the key used for transfering of sensative data
            //this is a required 
            formDefinition.CryptoKey = CryptoKey;

            //decalare a help uri, this is not required and will not be display if it is not declared
            formDefinition.HelpUri = new Uri("http://www.scribesoft.com");

            //for each input item on the screen a new entry definition will need to be created and 
            //added to the form definition in order to be serialized and passed up to the ui.
            //this first entry will define a combo box
            EntryDefinition entry = new EntryDefinition();

            //set the input type to plain text
            entry.InputType = InputType.Text;
            entry.IsRequired = true;

            //set the Label that is displayed to the user and the identifying property name
            //note: the label and the property name may be different
            entry.Label = "Provider";
            entry.PropertyName = "Provider";

            //set the position in which the control will be displayed
            entry.Order = 1;

            //the options will hold the list of values to be placed in the combo box
            //The first value is what will be displayed to the user
            //The second value is the actual value that will be passed back to the connector
            entry.Options.Add("SQL Server 2012", "SQLNCLI11");
            entry.Options.Add("SQL Server 2008", "SQLNCLI10");
            entry.Options.Add("SQL Server 2005", "SQLNCLI");
            //finally add the entry definition representing the combobox to the form definition
            formDefinition.Add(entry);

            //create and entry for a standard text box
            formDefinition.Add(new EntryDefinition { InputType = InputType.Text, PropertyName = "Server", Label = "Server", Order = 2, IsRequired = true });
            formDefinition.Add(new EntryDefinition { InputType = InputType.Text, PropertyName = "Database", Label = "Database", Order = 3, IsRequired = true });
            formDefinition.Add(new EntryDefinition { InputType = InputType.Text, PropertyName = "UserName", Label = "UserName", Order = 4, IsRequired = true });

            //Finally create a password field
            formDefinition.Add(new EntryDefinition { InputType = InputType.Password, PropertyName = "Password", Label = "Password", Order = 5, IsRequired = true });

            //serialize the formDefinition so that it can be sent back to the UI
            return formDefinition.Serialize();
        }

        /// <summary>
        /// Method to correctly log the result of the method 
        /// </summary>
        /// <param name="methodResult">Method Result to log</param>
        private void LogMethodResult(MethodResult methodResult)
        {
            string message;
            //Connector Api severity enumertion to define the level for logging
            Logger.Severity severity;

            //check if the result of the method was a success
            if (methodResult.Success)
            {
                //create a message to log in the event of a success
                message = "Method Executed Successfully";
                severity = Logger.Severity.Debug;
            }
            else
            {
                //create a message to log in the event of an error
                var errorInfo = methodResult.ErrorInfo;
                message = string.Format("An Error has occured in {0}: {2}Error Number:{1}{2}Error Description:{3}{2}Error Detail:{4}", Globals.ConnectorName, errorInfo.Number, Environment.NewLine, errorInfo.Description, errorInfo.Detail);
                severity = Logger.Severity.Error;
            }

            //log the result severity, the name of the connector, and the created message
            Logger.Write(severity, Globals.ConnectorName, message);

        }

        /// <summary>
        /// Method to correctly log the result of the operation 
        /// </summary>
        /// <param name="operationResult">Operation Result to log</param>
        private void LogOperationResult(OperationResult operationResult)
        {
            string message;
            //Connector Api severity enumertion to define the level for logging
            Logger.Severity severity;

            //check if the result of the method was a success
            if (operationResult.Success[0])
            {
                //create a message to log in the event of a success
                message = "Operation Executed Successfully";
                severity = Logger.Severity.Debug;
            }
            else
            {
                //create a message to log in the event of an error
                var errorInfo = operationResult.ErrorInfo[0];
                message = string.Format("An Error has occured in {0}: {2}Error Number:{1}{2}Error Description:{3}{2}Error Detail:{4}", Globals.ConnectorName, errorInfo.Number, Environment.NewLine, errorInfo.Description, errorInfo.Detail);
                severity = Logger.Severity.Error;
            }

            //log the result severity, the name of the connector, and the created message
            Logger.Write(severity, Globals.ConnectorName, message);

        }
        #endregion
    }
}
