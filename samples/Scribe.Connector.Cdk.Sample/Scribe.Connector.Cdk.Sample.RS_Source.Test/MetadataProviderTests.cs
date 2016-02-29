// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetadataProviderTests.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Cryptography;

namespace Scribe.Connector.Cdk.Sample.RS_Source.Test
{
    [TestClass]
    public class MetadataProviderTests
    {

        #region Metadata Provider Pre Test Setup
        private static readonly IConnector _rsSourceConnector = new RS_SourceConnector();
        private static IMetadataProvider _metadataProvider;

        /// <summary>
        /// Decryption key for securly passing sensitive data to the connector
        /// Note: this must be that same one both sides
        /// </summary>
        private static string CryptoKey
        {
            get { return "25650FCD-131E-4ECB-AD52-8180BE6D779D"; }
        }

        /// <summary>
        /// Prior to running any tests set the connection properties,
        /// perform the connect method in the connector,
        /// and set the metadata provider varible
        /// </summary>
        [ClassInitialize]
        public static void Startup(TestContext context)
        {
            var connectionProperties = new Dictionary<string, string>();
            //setup the initial parameters for data connection
            connectionProperties.Add("Provider", "SQLNCLI10");
            connectionProperties.Add("Server", "localhost");
            connectionProperties.Add("Database", "ScribeSampleRSSource");
            connectionProperties.Add("UserName", "sa");
            //encrypt the connection password using the shared key
            string encryptedPassword = Encryptor.Encrypt_AesManaged("sa", CryptoKey);
            connectionProperties.Add("Password", encryptedPassword);
            //open a new connection to the datasource
            _rsSourceConnector.Connect(connectionProperties);

            //set an instance of the open metadata provider
            _metadataProvider = _rsSourceConnector.GetMetadataProvider();
        }
        #endregion

        #region Metadata Provider Post Test Clean Up
        [ClassCleanup]
        public static void CleanUp()
        {
            //after all the test are run disconnect from the datasource
            _rsSourceConnector.Disconnect();
        }
        #endregion

        #region RetrieveObjectDefinitions Tests
        /// <summary>
        /// Validity test for retrieving the object definition list using the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionsValidTest()
        {
            //get the list of objects using the provider class
            var objectDefinitions = _metadataProvider.RetrieveObjectDefinitions();
            //check that objects have been returned
            Assert.AreNotEqual(0, objectDefinitions.Count());
        }
        #endregion

        #region RetrieveObjectDefinition Tests
        /// <summary>
        /// Validity test for retrieving a specific object definition using the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionValidTest()
        {
            //get the object definition specified
            var objectDefinition = _metadataProvider.RetrieveObjectDefinition("Addresses");
            //check that the object definition is not null
            Assert.IsNotNull(objectDefinition);
        }

        [TestMethod]
        public void RetrieveObjectDefinitionValidHiddenTest()
        {
            //get the object definition specified
            var objectDefinition = _metadataProvider.RetrieveObjectDefinition("ScribeChangeHistory");
            //check that the object definition is not null
            Assert.IsNotNull(objectDefinition);
            Assert.IsTrue(objectDefinition.Hidden);
        }

        /// <summary>
        /// Invalid test to show the result of an incorrect object name for object definition retrieval via the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionInValidTest()
        {
            //get the object definition using a bogus object name
            var objectDefinition = _metadataProvider.RetrieveObjectDefinition("xxxxx");
            //check that the object definition is null since a valid object was not passed in
            Assert.IsNull(objectDefinition);
        }
        #endregion
    }
}
