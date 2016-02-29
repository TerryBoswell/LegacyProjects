using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Core.ConnectorApi.Metadata;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Test
{
	[TestClass]
	public class MetadataProviderTest
	{

		/// <summary>
		/// Verifies that we get the expected supported actions from the class
		/// </summary>
		[TestMethod]
		public void MetadataProvider_RetrieveActionDefinitions_Test()
		{

			//arrange: 
			MetadataProvider provider = new MetadataProvider();

			//act
			IEnumerable<IActionDefinition> actions = provider.RetrieveActionDefinitions();

			//assert: 
			Assert.AreEqual(2, actions.Count());

			//Extract by the name, match by the Enum
			Assert.AreEqual(KnownActions.Create, actions.FirstOrDefault(action => action.FullName == "Create").KnownActionType);
			Assert.AreEqual(KnownActions.Query, actions.FirstOrDefault(action => action.FullName == "Query").KnownActionType);

		}

		/// <summary>
		/// Unit test to run the RetrieveObjectDefinitions method
		/// </summary>
		[TestMethod]
		public void MetadataProvider_RetrieveObjectDefinitions_Test()
		{
			
			//arrange
			MetadataProvider provider = new MetadataProvider();

			//act
			//call using default parameters
			IEnumerable<IObjectDefinition> entities = provider.RetrieveObjectDefinitions();

			//assert: 
			//use linq to pull out the full name and make sure it matches:
			Assert.AreEqual(2, entities.Count());
			Assert.AreEqual("Registrant", entities.FirstOrDefault(entitiy => entitiy.FullName == "Registrant").FullName);
			Assert.AreEqual("Webinar", entities.FirstOrDefault(entity => entity.FullName == "Webinar").FullName);

		}

		/// <summary>
		/// Unit test to run the RetrieveObjectDefinition method
		/// </summary>
		[TestMethod]
		public void MetadataProvider_RetrieveObjectDefinition_Test()
		{

			//arrange
			MetadataProvider provider = new MetadataProvider();

			//act:
			IObjectDefinition entities = provider.RetrieveObjectDefinition("Registrant");

			//assert: 
			Assert.AreEqual("Registrant", entities.FullName);

		}

		/// <summary>
		/// Tests to make sure we retrieve the correct fields for each entity
		/// </summary>
		[TestMethod]
		public void MetadataProvider_RetrieveObjectDefinitionsWithPropertyDefinitions_Test()
		{
			//arrange: 
			MetadataProvider provider = new MetadataProvider();

			//act:
			IEnumerable<IObjectDefinition> entities = provider.RetrieveObjectDefinitions(true);

			//Assert: 
			var registrant = entities.FirstOrDefault(entity => entity.FullName == "Registrant");
			var webinar = entities.FirstOrDefault(entity => entity.FullName == "Webinar");

			//spotcheck the results
			Assert.AreEqual(9, registrant.PropertyDefinitions.Count);
			Assert.IsTrue(registrant.PropertyDefinitions.Any(fields => fields.FullName.Contains("Email")));

			Assert.AreEqual(8, webinar.PropertyDefinitions.Count);
			Assert.IsTrue(webinar.PropertyDefinitions.Any(fields => fields.FullName.Contains("WebinarKey")));

		}

		/// <summary>
		/// Test to make sure we can retrieve a single entity and its fields
		/// </summary>
		[TestMethod]
		public void MetadataProvider_RetrieveObjectDefinitionWithPropertyDefinitions_Test()
		{
			
			//arrange
			MetadataProvider provider = new MetadataProvider();

			//act:
			var registrant = provider.RetrieveObjectDefinition("Registrant", true);

			//assert: 
			Assert.AreEqual(9, registrant.PropertyDefinitions.Count);
			Assert.IsTrue(registrant.PropertyDefinitions.Any(fields => fields.FullName.Contains("RegistrantKey")));

		}


	}
}
