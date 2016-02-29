// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SYSConnector.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.ConnectionUI;
using Scribe.Core.ConnectorApi.Cryptography;
using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Core.ConnectorApi.Query;

namespace Scribe.Connector.Cdk.Sample.SYS
{
    [ScribeConnector(ConnectorTypeIdAsString, ConnectorTypeName, ConnectorTypeDescription, typeof(SYSConnector), SettingsUITypeName,
    SettingsUIVersion, ConnectionUITypeName, ConnectionUIVersion, XapFileName, new string[] { "Scribe.IS.Source", "Scribe.IS.Target" })]
    public class SYSConnector : IConnector
    {
        #region private members
        private string _connectionString;
        private MetadataProvider _metadataProvider;
        private OleDbMetadataAccess _metadataAccess;
        private OleDbDataAccess _dataAccess = new OleDbDataAccess();
        /// <summary>
        /// Decryption key for securly passing sensitive data to the connector
        /// Note: this must be that same one both sides
        /// </summary>
        private static string CryptoKey
        {
            get { return "4DCE8727-E3B8-4BC8-92B5-23CBD343663B"; }
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
        internal const string ConnectorTypeIdAsString = "877F719A-BAE4-4BA8-A248-7F9D316BAABB";

        /// <summary>
        ///   The name of the Connector Type.
        /// </summary>
        internal const string ConnectorTypeName = "CDK SYS Sample";

        /// <summary>
        ///   The description of the Connector Type.
        /// </summary>
        internal const string ConnectorTypeDescription = "Cdk SYS Sample from Scribe.";

        /// <summary>
        /// Default metadata prefix
        /// </summary>
        internal const string MetadataPrefix = "CdkSYSSample";
        #endregion

        #region public properties
        /// <summary>
        ///   This constant holds this Connector Type Id. This is a GUID that is used to verify the Connector
        /// </summary>
        public Guid ConnectorTypeId
        {
            get { return Guid.Parse(ConnectorTypeIdAsString); }
        }
        #endregion

        #region Implementation of IConnector
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
            // When wrapped in a using statement, the exit point 
            // is written during garbage collection.
            using (new LogMethodExecution(ConnectorTypeName, "Connect"))
            {
                try
                {
                    //parse the incomming properties to ensure the proper parameters are set
                    BuildConnectionString(properties);

                    //attempt a connection to the selected datasource
                    _dataAccess.OleDbConnect(_connectionString);

                    //open the connection for the metadata access to the server
                    _metadataAccess = new OleDbMetadataAccess(_dataAccess);

                    //open the connection to the metadata provider
                    _metadataProvider = new MetadataProvider(_metadataAccess);

                }
                catch (OleDbException oleDbException)
                {
                    string msg = ExceptionFormatter.BuildActionableException(
                        "Unable to Connect to datasource",
                        string.Format("The following error occured in the {0}:",
                        Globals.ConnectorName), oleDbException);

                    //be sure to log any errors that occure while attempting to connect
                    Logger.Write(Logger.Severity.Error, Globals.ConnectorName, msg);
                    //throw the InvalidConnectionException found in the ConnectorApi
                    throw new InvalidConnectionException(msg);
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    string msg = ExceptionFormatter.BuildActionableException(
                        "Unable to connect to datasource the provider information is invalid.",
                        string.Format("The following error occured in the {0}:", Globals.ConnectorName),
                                                         invalidOperationException);
                    //be sure to log an error that is due to an invalid provider
                    Logger.Write(Logger.Severity.Error, Globals.ConnectorName, msg);
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
                _dataAccess.OleDbDisconnect();
            }
        }

        /// <summary>
        /// This method will execute a Method returning a result. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public MethodResult ExecuteMethod(MethodInput input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method will execute an operation returning a result. 
        /// This method is also used in bulk operations.
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

                Globals.OperationType operationType;

                //retrieve the operation type from the local enumeration
                Enum.TryParse(input.Name, true, out operationType);

                try
                {
                    // Use the name stored in the operation 
                    // input to determine the correct operation to execute
                    switch (operationType)
                    {
                        case Globals.OperationType.Delete:
                            operationResult = operationHandler.DeleteOperation(input);
                            break;
                        case Globals.OperationType.Create:
                            operationResult = operationHandler.CreateOperation(input);
                            break;
                        case Globals.OperationType.Update:
                            operationResult = operationHandler.UpdateOperation(input);
                            break;
                        case Globals.OperationType.Upsert:
                            operationResult = operationHandler.UpsertOperation(input, _metadataAccess);
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
            // Use LogMethodExecution to add entry and exit tracing to a method. 
            // When wrapped in a using statement, the exit point 
            // is written during garbage collection.
            using (new LogMethodExecution(Globals.ConnectorName, "ExecuteQuery"))
            {
                //set the enumerated list of data entities to null
                IEnumerable<DataEntity> dataEntities = null;

                try
                {
                    //Verify that a root entity is decalred and that return data is requested in the property list.
                    if (string.IsNullOrWhiteSpace(query.RootEntity.ObjectDefinitionFullName))
                    {
                        //this message can be anything that is meaningfull to the user
                        string message = string.Format(ErrorCodes.InvalidQueryObject.Description,
                                                       query.RootEntity.PropertyList.Count);
                        //Log that no query has been filled out
                        Logger.Write(Logger.Severity.Error, "Execute Query Failed", message);
                        throw new ArgumentException(message);
                    }

                    //retrieve the name of the root entity
                    string tableName = query.RootEntity.ObjectDefinitionFullName;

                    //retrieve the list of column definitions for proper data type convertion
                    DataTable columnDefinitions = _metadataAccess.GetColumnDefinitions(tableName);
                    columnDefinitions.TableName = tableName;

                    //Create a new instance of the query builder and send the query information along
                    SqlQueryBuilder queryBuilder = new SqlQueryBuilder(query, columnDefinitions);

                    //Convert the query builder to a string value
                    string queryString = queryBuilder.ToString();

                    //Execute the query and retrieve the enumerated results
                    dataEntities = _dataAccess.Execute(query.RootEntity.ObjectDefinitionFullName, queryString, queryBuilder.RelatedForeignKeys);

                    if (dataEntities != null)
                    {
                        //Since the dataEntities is an enumerated list it will need to be forced to fire the execute method
                        foreach (var entity in dataEntities)
                        {
                            break;
                        }
                    }
                }
                catch (FatalErrorException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    //this message may be anything that is meaningful to the user
                    string message = string.Format("{0} {1}", ErrorCodes.GenericConnectorError, exception.Message);
                    //Log the exception
                    Logger.Write(Logger.Severity.Error, message, exception.StackTrace);
                    //All exeptions that occur in the query execution must be returned as an InvalidQueryException
                    throw new InvalidExecuteQueryException(message);
                }

                return dataEntities;
            }
        }

        /// <summary>
        /// get the open instance of the metadata provider
        /// </summary>
        /// <returns>Currenct open instance of the MetadataProvider</returns>
        public IMetadataProvider GetMetadataProvider()
        {
            return _metadataProvider;
        }

        /// <summary>
        /// Gets the value indicating whether the Connector is connected
        /// </summary>
        public bool IsConnected
        {
            get { return _dataAccess.IsConnected; }
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
            //create a new instance of the built-in form definition object
            FormDefinition formDefinition = new FormDefinition();
            //Add your company name here, this is a required field 
            formDefinition.CompanyName = "Scribe Software Corporation © 1996-2011 All rights reserved";
            //declare the key used for transfering of sensative data
            //this is a required 
            formDefinition.CryptoKey = CryptoKey;

            //decalare a help uri, this is not required and 
            //will not be display if it is not declared
            formDefinition.HelpUri = new Uri("http://www.scribesoft.com");

            //for each input item on the screen a new entry definition 
            //will need to be created and added to the form definition 
            //in order to be serialized and passed up to the ui.
            //this first entry will define a combo box
            EntryDefinition entry = new EntryDefinition();

            //set the input type to plain text
            entry.InputType = InputType.Text;
            entry.IsRequired = true;

            //set the Label that is displayed to the user and 
            //the identifying property name
            //note: the label and the property name may be different
            entry.Label = "Provider";
            entry.PropertyName = "Provider";

            //set the position in which the control will be displayed
            entry.Order = 1;

            //the options will hold the list of values to be placed in the combo box
            //The first value is what will be displayed to the user
            //The second value is the actual value that 
            //will be passed back to the connector
            entry.Options.Add("SQL Server 2012", "SQLNCLI11");
            entry.Options.Add("SQL Server 2008", "SQLNCLI10");
            entry.Options.Add("SQL Server 2005", "SQLNCLI");
            //finally add the entry definition representing the combobox
            formDefinition.Add(entry);

            //create and entry for a standard text box
            formDefinition.Add(new EntryDefinition
            {
                InputType = InputType.Text,
                PropertyName = "Server",
                Label = "Server",
                Order = 2,
                IsRequired = true
            });

            formDefinition.Add(new EntryDefinition
            {
                InputType = InputType.Text,
                PropertyName = "Database",
                Label = "Database",
                Order = 3,
                IsRequired = true
            });

            formDefinition.Add(new EntryDefinition
            {
                InputType = InputType.Text,
                PropertyName = "UserName",
                Label = "UserName",
                Order = 4,
                IsRequired = true
            });

            //Finally create a password field
            formDefinition.Add(new EntryDefinition
            {
                InputType = InputType.Password,
                PropertyName = "Password",
                Label = "Password",
                Order = 5,
                IsRequired = true
            });

            //serialize the formDefinition so that it can be sent back to the UI
            return formDefinition.Serialize();
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
