// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetadataProvider.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Connector.Cdk.Sample.Rest.Sys.Entities;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys
{

	/// <summary>
	/// Compiles and stores any metadata provided by the REST service. 
	/// </summary>
	public class MetadataProvider : IMetadataProvider
	{

		#region Member Properties

		/// <summary>
		/// Contains the collection of Entity types. Used to generate Metadata for Scribe Online.
		/// </summary>
		private Dictionary<string, Type> EntityCollection = new Dictionary<string, Type>();

		#endregion

		/// <summary>
		/// Creates an instance of the MetadataProvider and prepopulates a dictionary with the Entity types we want to register.
		/// </summary>
		public MetadataProvider()
		{

			//Fill the Entity collection. We'll use this to create metadata when Scribe Online asks for it:
			EntityCollection = PopulateEntityCollection();

		}

		#region IMetadataProvider Implementation

		public void ResetMetadata()
		{
			//no-op; we're not caching metadata in this example.
		}

		/// <summary>
		/// Retrieve a list of Connector-supported CRUD actions. 
		/// These actions will be reflected in the operations that will be executed from
		/// 'ExecuteOperation' method found in the IConnector implemented class.
		/// Note: Object level action support is defined in RetrieveObjectDefinition and RetrieveObjectDefinitions.
		/// </summary>
		/// <returns>A collection of IActionDefinition that this connect supports</returns>
		public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
		{

			//GoToWebinar only supports Create and Retrieve. So we will hand those back as supported actions.
			//Build and hand back a hard-coded list since this won't change.

			using (LogMethodExecution logger = new LogMethodExecution("Rest CDK Example", "RetrieveActionDefinitions"))
			{
				var actionDefinitions = new List<IActionDefinition>();
				var createDef = new ActionDefinition
				{
					SupportsInput = true,
					KnownActionType = KnownActions.Create,
					SupportsBulk = false,
					FullName = KnownActions.Create.ToString(),
					Name = KnownActions.Create.ToString(),
					Description = string.Empty
				};

				actionDefinitions.Add(createDef);

				var queryDef = new ActionDefinition
				{
					SupportsConstraints = true,
					SupportsRelations = false,
					SupportsLookupConditions = false,
					SupportsSequences = false,
					KnownActionType = KnownActions.Query,
					SupportsBulk = false,
					Name = KnownActions.Query.ToString(),
					FullName = KnownActions.Query.ToString(),
					Description = string.Empty
				};

				actionDefinitions.Add(queryDef);

				return actionDefinitions; 
			}
		}

		public IMethodDefinition RetrieveMethodDefinition(string objectName, bool shouldGetParameters = false)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(bool shouldGetParameters = false)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieves all ObjectDefinitions that this connector offers.
		/// </summary>
		/// <param name="shouldGetProperties"></param>
		/// <param name="shouldGetRelations"></param>
		/// <returns></returns>
		public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(bool shouldGetProperties = false, bool shouldGetRelations = false)
		{
			//Loop through our collection of entities and let the other methods create the Metadata as they need it.
			using (LogMethodExecution logger = new LogMethodExecution("Rest CDK Example", "RetrieveObjectDefinitions"))
			{
				foreach (var entityType in EntityCollection)
				{
					yield return RetrieveObjectDefinition(entityType.Key, shouldGetProperties, shouldGetRelations);
				} 
			}
		}

		/// <summary>
		/// Retrieves a single ObjectDefinition, by name.
		/// </summary>
		/// <param name="objectName"></param>
		/// <param name="shouldGetProperties"></param>
		/// <param name="shouldGetRelations"></param>
		/// <returns></returns>
		public IObjectDefinition RetrieveObjectDefinition(string objectName, bool shouldGetProperties = false, bool shouldGetRelations = false)
		{

			//GoToWebinar doesn't have any entities with parent/child relationships. 
			//As such, we're not using the 'shouldGetRelations' parameter

			IObjectDefinition objectDefinition = null;

			using (LogMethodExecution logger = new LogMethodExecution("Rest CDK Example", "RetrieveObjectDefinition"))
			{

				//extract the one type that Scribe Online is asking for: 
				if (EntityCollection.Count > 0)
				{
					foreach (var keyValuePair in EntityCollection)
					{
						if (keyValuePair.Key == objectName)
						{
							Type entityType = keyValuePair.Value;
							if (entityType != null)
							{
								//hand the type down to our reflection method and create an IObjectDefiniton for Scribe Online
								objectDefinition = GetObjectDefinition(entityType, shouldGetProperties);
							}
						}
					} 
				}

			}

			return objectDefinition;

		}

		#endregion

		public void Dispose()
		{
			//no-op; we're not holding onto any cache or in-memory entities
		}

		#region Private Helper Methods

		/// <summary>
		/// Uses reflection to generate Metadata from the in-memory entities.
		/// </summary>
		/// <param name="entityType">The entity type from which to create metadata.</param>
		/// <param name="shouldGetFields">Whether or not this method should get the entity's fields.</param>
		/// <returns>A single IObjectDefinition that represents the entity</returns>
		private IObjectDefinition GetObjectDefinition(Type entityType, bool shouldGetFields)
		{

			IObjectDefinition objectDefinition = null;

			objectDefinition = new ObjectDefinition
			{
				Name = entityType.Name,
				FullName = entityType.Name,
				Description = string.Empty,
				Hidden = false,
				RelationshipDefinitions = new List<IRelationshipDefinition>(),
				PropertyDefinitions = new List<IPropertyDefinition>(),
				SupportedActionFullNames = new List<string>()
			};

			objectDefinition.SupportedActionFullNames.Add("Query");

			if (entityType.Name == "Registrant")
			{
				
				/* If we're building the 'Registrant' metadata
				 * add the 'Create' action to it. 
				 * Other entities do not support create.
				 */

				objectDefinition.SupportedActionFullNames.Add("Create");
				 
			}

			if (shouldGetFields)
			{
				objectDefinition.PropertyDefinitions = GetFieldDefinitions(entityType);
			}

			return objectDefinition;

		}

		/// <summary>
		/// Uses reflection to pull the Fields from the Entities for Scribe OnLine
		/// </summary>
		/// <param name="entityType"></param>
		/// <returns></returns>
		private List<IPropertyDefinition> GetFieldDefinitions(Type entityType)
		{

			var fields = new List<IPropertyDefinition>();

			//Pull a collection from the incoming entity:
			var fieldsFromType = entityType.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy |
				BindingFlags.Public | BindingFlags.GetProperty);

			foreach (var field in fieldsFromType)
			{

				var propertyDefinition = new PropertyDefinition
				{
						Name = field.Name,
						FullName = field.Name,
						PropertyType = field.PropertyType.ToString(),
						PresentationType = field.PropertyType.ToString(),
						Nullable = false,
						IsPrimaryKey = false,
						UsedInQueryConstraint = true,
						UsedInQuerySelect = true,
						UsedInActionOutput = true,
						UsedInQuerySequence = true,
						Description = field.Name,
					};

				//Find any of the following fields that may be attached to the entity. 
				//These are defined by the attributes attached to the property
				foreach (var attribute in field.GetCustomAttributes(false))
				{

					//whether the field is readonly or not
					if (attribute is ReadOnlyAttribute)
					{
						var readOnly = (ReadOnlyAttribute)attribute;
						propertyDefinition.UsedInActionInput = readOnly == null || !readOnly.IsReadOnly;
					}

					//whether the field is required to be populated on an insert
					if (attribute is RequiredAttribute)
					{
						propertyDefinition.RequiredInActionInput = true;
					}

					//if the field can be used as a match field for the query
					if (attribute is KeyAttribute)
					{
						propertyDefinition.UsedInLookupCondition = true;
					}

				}

				fields.Add(propertyDefinition);

			}
			
			return fields;

		}

		/// <summary>
		/// Fills a dictionary with the entities we want to hand back to Scribe Online. 
		/// </summary>
		/// <returns></returns>
		private Dictionary<string, Type> PopulateEntityCollection()
		{
			
			/* GoToWebinar doesn't allow us to create custom entities
			 * To keep this simple, we'll hard code these here. 
			 * We can add more entities to this list later if needed 
			 * See the other CDK examples for other ways to handle Entities in Metadata
			 */
			
			Dictionary<string, Type> entities = new Dictionary<string, Type>();

			entities.Add("Registrant", typeof(Registrant));
			entities.Add("Webinar", typeof(Webinar));
			entities.Add("UpcomingWebinar", typeof(UpcomingWebinar));

			return entities;

		}

		#endregion
		
	}
}
