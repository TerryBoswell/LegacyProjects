// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RS_SourceConnectorTest.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Scribe.Core.ConnectorApi.Exceptions;

namespace Scribe.Connector.Cdk.Sample.RS_Source.Test
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Core.ConnectorApi.Cryptography;

    [TestClass]
    public class RS_SourceConnectorTest
    {
        #region RS Source Connector Pre Test Setup
        /// <summary>
        /// Create a new dictionary to store the connection properties
        /// </summary>
        private Dictionary<string, string> _connectionProperties = new Dictionary<string, string>();
        private static RS_SourceConnector _rsSourceConnector = new RS_SourceConnector();

        private const string InvalidPropertyValue = "xxxxxx";

        /// <summary>
        /// Decryption key for securly passing sensitive data to the connector
        /// Note: this must be that same one both sides
        /// </summary>
        private static string CryptoKey
        {
            get { return "25650FCD-131E-4ECB-AD52-8180BE6D779D"; }
        }

        /// <summary>
        /// Setup the initial connection properties prior to performing any tests
        /// </summary>
        [TestInitialize]
        public void StartUp()
        {
            //setup the initial parameters for data connection
            _connectionProperties.Add("Provider", "SQLNCLI10");
            _connectionProperties.Add("Server", "localhost");
            _connectionProperties.Add("Database", "ScribeSampleRSSource");
            _connectionProperties.Add("UserName", "sa");
            //encrypt the connection password using the shared key
            string encryptedPassword = Encryptor.Encrypt_AesManaged("sa", CryptoKey);
            _connectionProperties.Add("Password", encryptedPassword);
        }
        #endregion

        #region RS Source Connector Post Test Cleanup
        [ClassCleanup]
        public static void Cleanup()
        {
            if (_rsSourceConnector.IsConnected)
            {
                _rsSourceConnector.Disconnect();
            }
        }
        #endregion

        #region Valid PreConnect Test
        /// <summary>
        /// This is a validity test to ensure that serialized data is retrieved from the preconnect method, 
        /// this is the data that will be read from the UI
        /// </summary>
        [TestMethod]
        public void RsSourceConnectorPreConnectValidTest()
        {
            //create a new instance of the sample connector
            var rsSourceConnector = new RS_SourceConnector();
            //get the ui definition
            string preConnectString = rsSourceConnector.PreConnect(new Dictionary<string, string>());
            //check that the definition has been received
            Assert.AreNotSame(string.Empty, preConnectString);
        }

        #endregion

        #region Valid Connection Tests
        /// <summary>
        /// This is to test the third-party connection
        /// </summary>
        [TestMethod]
        public void RsSourceConnectorConnectionValidTest()
        {
            //create a new instance of the sample connector
            var rsSourceConnector = new RS_SourceConnector();

            //call the connect method from the connector and pass in the connection properties dictionary
            rsSourceConnector.Connect(_connectionProperties);

            //do a check that the IsConnected flag is true ad the connection has been opened
            Assert.IsTrue(rsSourceConnector.IsConnected);
        }

        [TestMethod]
        public void RsSourceConnectorDisconnectValidTest()
        {
            //create a new instance of the sample connector
            var rsSourceConnector = new RS_SourceConnector();

            //call the connect method from the connector and pass in the connection properties dictionary
            rsSourceConnector.Connect(_connectionProperties);

            //do a check that the IsConnected flag is true
            Assert.IsTrue(rsSourceConnector.IsConnected);

            //call the disconnect method from the connector
            rsSourceConnector.Disconnect();

            //do a check the connector IsConnected flag is false
            Assert.IsFalse(rsSourceConnector.IsConnected);
        }
        #endregion

        #region Invalid Connection Tests
        /// <summary>
        /// Test the result of an invalid Provider name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidConnectionException))]
        public void RsSourceConnectorConnectInvalidProviderTest()
        {
            //change the provider information to an invalid type
            _connectionProperties.Remove("Provider");
            _connectionProperties.Add("Provider", InvalidPropertyValue);

            //call the connect method from the connector and pass in the connection properties dictionary
            _rsSourceConnector.Connect(_connectionProperties);
        }

        /// <summary>
        /// Test the result of an invalid server location
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidConnectionException))]
        public void RsSourceConnectorConnectInvalidServerTest()
        {
            //change the server information to an invalid type
            _connectionProperties.Remove("Server");
            _connectionProperties.Add("Server", InvalidPropertyValue);

            //call the connect method from the connector and pass in the connection properties dictionary
            _rsSourceConnector.Connect(_connectionProperties);
        }

        /// <summary>
        /// Test the result of an invalid Database name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidConnectionException))]
        public void RsSourceConnectorConnectInvalidDatabaseTest()
        {
            //change the provider information to an invalid type
            _connectionProperties.Remove("Database");
            _connectionProperties.Add("Database", InvalidPropertyValue);

            //call the connect method from the connector and pass in the connection properties dictionary
            _rsSourceConnector.Connect(_connectionProperties);
        }

        /// <summary>
        /// Test the result of an invalid user name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidConnectionException))]
        public void RsSourceConnectorConnectInvalidUserNameTest()
        {
            //change the username information to an invalid type
            _connectionProperties.Remove("UserName");
            _connectionProperties.Add("UserName", InvalidPropertyValue);

            //call the connect method from the connector and pass in the connection properties dictionary
            _rsSourceConnector.Connect(_connectionProperties);
        }

        /// <summary>
        /// Test the result of an invalid Provider name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidConnectionException))]
        public void RsSourceConnectorConnectInvalidPasswordTest()
        {
            //change the password information to an invalid type
            _connectionProperties.Remove("Password");
            string encryptedPassword = Encryptor.Encrypt_AesManaged(InvalidPropertyValue, CryptoKey);
            _connectionProperties.Add("Password", encryptedPassword);

            //call the connect method from the connector and pass in the connection properties dictionary
            _rsSourceConnector.Connect(_connectionProperties);
        }
        #endregion
    }
}
