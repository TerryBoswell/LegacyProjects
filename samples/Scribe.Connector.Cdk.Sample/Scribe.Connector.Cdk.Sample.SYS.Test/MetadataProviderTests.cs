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

namespace Scribe.Connector.Cdk.Sample.SYS.Test
{
    /// <summary>
    /// Summary description for MetadataProviderTests
    /// </summary>
    [TestClass]
    public class MetadataProviderTests
    {
        #region Metadata Provider Pre Test Setup
        private static readonly IConnector _sysConnector = new SYSConnector();
        private static IMetadataProvider _metadataProvider;

        /// <summary>
        /// Decryption key for securly passing sensitive data to the connector
        /// Note: this must be that same one both sides
        /// </summary>
        private static string CryptoKey
        {
            get { return "4DCE8727-E3B8-4BC8-92B5-23CBD343663B"; }
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
            _sysConnector.Connect(connectionProperties);

            //set an instance of the open metadata provider
            _metadataProvider = _sysConnector.GetMetadataProvider();
        }
        #endregion

        #region Metadata Provider Post Test Clean Up
        [ClassCleanup]
        public static void CleanUp()
        {
            //after all the test are run disconnect from the datasource
            _sysConnector.Disconnect();
        }
        #endregion

        #region RetrieveObjectDefinitions Tests
        /// <summary>
        /// Validity test for retrieving the object definition list using the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionsNoPropertiesValidTest()
        {
            //get the list of objects using the provider class
            var objectDefinitions = _metadataProvider.RetrieveObjectDefinitions();
            //check that objects have been returned
            Assert.AreNotEqual(0, objectDefinitions.Count());
            //check if the list of actions has been retrieved
            Assert.AreNotEqual(0, objectDefinitions.ElementAt(0).SupportedActionFullNames.Count);
        }

        /// <summary>
        /// Validity test for retrieving the object definition list using the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionsWithPropertiesValidTest()
        {
            //get the list of objects using the provider class
            var objectDefinitions = _metadataProvider.RetrieveObjectDefinitions(true);
            //check that objects have been returned
            Assert.AreNotEqual(0, objectDefinitions.Count());
            //make sure property definitions are returned
            Assert.AreNotEqual(0, objectDefinitions.ElementAt(0).PropertyDefinitions.Count());
            //check that no relationship defintions have been retrieved
            Assert.AreEqual(0, objectDefinitions.ElementAt(0).RelationshipDefinitions.Count());
            //check if the list of actions has been retrieved
            Assert.AreNotEqual(0, objectDefinitions.ElementAt(0).SupportedActionFullNames.Count);
        }

        /// <summary>
        /// Validity test for retrieving the object definition list using the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionsWithRelationsValidTest()
        {
            //get the list of objects using the provider class
            var objectDefinitions = _metadataProvider.RetrieveObjectDefinitions(false, true);
            //check that objects have been returned
            Assert.AreNotEqual(0, objectDefinitions.Count());
            //make sure no property definitions are returned
            Assert.AreEqual(0, objectDefinitions.ElementAt(0).PropertyDefinitions.Count());
            //check that relationship defintions have been retrieved
            Assert.AreNotEqual(0, objectDefinitions.ElementAt(0).RelationshipDefinitions.Count());
            //check if the list of actions has been retrieved
            Assert.AreNotEqual(0, objectDefinitions.ElementAt(0).SupportedActionFullNames.Count);
        }

        /// <summary>
        /// Validity test for retrieving the object definition list using the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionsWithRelationsAndPropertiesValidTest()
        {
            //get the list of objects using the provider class
            var objectDefinitions = _metadataProvider.RetrieveObjectDefinitions(true, true);
            //check that objects have been returned
            Assert.AreNotEqual(0, objectDefinitions.Count());
            //make sure property definitions are returned
            Assert.AreNotEqual(0, objectDefinitions.ElementAt(0).PropertyDefinitions.Count());
            //check that relationship defintions have been retrieved
            Assert.AreNotEqual(0, objectDefinitions.ElementAt(0).RelationshipDefinitions.Count());
            //check if the list of actions has been retrieved
            Assert.AreNotEqual(0, objectDefinitions.ElementAt(0).SupportedActionFullNames.Count);
        }
        #endregion

        #region RetrieveObjectDefinition Tests
        /// <summary>
        /// Validity test for retrieving a specific object definition using the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionNotPropertiesValidTest()
        {
            //get the object definition specified
            var objectDefinition = _metadataProvider.RetrieveObjectDefinition("Addresses");
            //check that the object definition is not null
            Assert.IsNotNull(objectDefinition);
            //Check that action definitions have been returned in the object definition
            Assert.AreNotEqual(0,objectDefinition.SupportedActionFullNames.Count);
        }

        /// <summary>
        /// Validity test for retrieving a specific object definition using the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionWithPropertiesValidTest()
        {
            //get the object definition specified
            var objectDefinition = _metadataProvider.RetrieveObjectDefinition("Addresses",true);
            //check that the object definition is not null
            Assert.IsNotNull(objectDefinition);
            //Check that property definitions are retrieved in the object definition
            Assert.AreNotEqual(0, objectDefinition.PropertyDefinitions.Count());
            //Check that action definitions have been returned for the object definition
            Assert.AreNotEqual(0, objectDefinition.SupportedActionFullNames.Count);
        }

        /// <summary>
        /// Validity test for retrieving a specific object definition using the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionWithRelationsValidTest()
        {
            //get the object definition specified
            var objectDefinition = _metadataProvider.RetrieveObjectDefinition("SalesOrders", false, true);
            //check that the object definition is not null
            Assert.IsNotNull(objectDefinition);
            //Check that property definitions are not retrieved in the object definition
            Assert.AreEqual(0, objectDefinition.PropertyDefinitions.Count());
            //Check that relationship definitions are retrieved in the object definition
            Assert.AreNotEqual(0, objectDefinition.RelationshipDefinitions.Count());
            //Check that action definitions have been returned for the object definition
            Assert.AreNotEqual(0, objectDefinition.SupportedActionFullNames.Count);
        }

        /// <summary>
        /// Validity test for retrieving a specific object definition using the metadata provider
        /// </summary>
        [TestMethod]
        public void RetrieveObjectDefinitionWithRelationsAndPropertiesValidTest()
        {
            //get the object definition specified
            var objectDefinition = _metadataProvider.RetrieveObjectDefinition("SalesOrders", true, true);
            //check that the object definition is not null
            Assert.IsNotNull(objectDefinition);
            //Check that property definitions have been returned for the object definition
            Assert.AreNotEqual(0, objectDefinition.PropertyDefinitions.Count());
            //Check that relationship definitions have been returned for the object definition
            Assert.AreNotEqual(0, objectDefinition.RelationshipDefinitions.Count());
            //Check that action definitions have been returned for the object definition
            Assert.AreNotEqual(0, objectDefinition.SupportedActionFullNames.Count);
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

        #region RetrieveActionDefinitions Tests

        /// <summary>
        /// This is a validity test to ensure action definitions are returned from the connector
        /// </summary>
        [TestMethod]
        public void RetrieveActionDefinitionsValidTest()
        {
            //Get the list of action definitions using the metadata provider
            var actionDefinitions = _metadataProvider.RetrieveActionDefinitions();
            //Check that an action definitions list was returned from the connector
            Assert.IsNotNull(actionDefinitions);
            //Check that the list of actions have been populated
            Assert.AreNotEqual(0, actionDefinitions.Count());
        }

        #endregion
    }
}
