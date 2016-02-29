// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryProcessor.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi;
using System.Reflection;
using Scribe.Connector.Cdk.Sample.Rest.Sys.Entities;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys
{

	/// <summary>
	/// Processes operation requests from Scribe Online
	/// </summary>
	public class OperationProcessor
	{

		private readonly IGoToWebinarClient _goToWebinarClient;
		private readonly IDictionary<string, string> _connectionInfo;

		public OperationProcessor(IGoToWebinarClient goToWebinarClient, IDictionary<string, string> connectionInfo)
		{
			this._goToWebinarClient = goToWebinarClient;
			this._connectionInfo = connectionInfo;
		}

		/// <summary>
		/// Processes the operation requests from Scribe Online
		/// </summary>
		/// <param name="operationInput">The operation request as handed down from the Connection class</param>
		/// <returns></returns>
		public OperationResult ExecuteOperation(OperationInput operationInput)
		{

			var operationResult = new OperationResult();
			// arrays to keep track of the operations statuses
			var operationSuccess = new List<bool>();
			var entitiesAffected = new List<int>();
			var errorList = new List<ErrorResult>();
			var entities = new List<DataEntity>();

			switch (operationInput.Name)
			{
					//This Connector only supports 'Create' and only on the Registrant entity: 
				case "Create":

					//Process each request individually: 
					foreach (var scribeEntity in operationInput.Input)
					{

						var returnEntity = new DataEntity();

						try
						{
							if (scribeEntity.ObjectDefinitionFullName == "Registrant")
							{
								//user the reflection method to create a Registrant object from the scribeEntity
								var registrant = EntityToObject<Registrant>(scribeEntity);

								//Hand it to the 'Create' method:
								returnEntity = CreateRegistrant(registrant);
							}
							else
							{
								throw new InvalidOperationException(String.Format("Create not supported for {0}", scribeEntity.ObjectDefinitionFullName));
							}
							//Successful operations
							operationSuccess.Add(true);
							entitiesAffected.Add(1);
							entities.Add(returnEntity);
						}
						catch (Exception ex) 
						{
							
							//add an error to our collections: 
							var errorResult = new ErrorResult
							{
								Description = string.Format("An error returned from GoToWebinar while creating Registrant: {0}", ex.Message),
								Detail = ex.StackTrace,
							};

							operationSuccess.Add(false);
							errorList.Add(errorResult);
							entitiesAffected.Add(0);
							//don't throw, allows us to keep moving through the requests.
						}

					}

					break;

				default:
					throw new InvalidOperationException("Invalid Operation");

			}

			//Completed the requests, hand back the results: 
			operationResult.Success = operationSuccess.ToArray();
			operationResult.ObjectsAffected = entitiesAffected.ToArray();
			operationResult.ErrorInfo = errorList.ToArray();
			operationResult.Output = entities.ToArray();

			return operationResult;

		}

		/// <summary>
		/// Converts a Scribe Online DataEntity into a GoToWebinar Entity using reflection
		/// </summary>
		/// <typeparam name="T">The GoToWebinar entity type</typeparam>
		/// <param name="scribeEntity">Scribe Online DataEntity</param>
		/// <returns></returns>
		private T EntityToObject<T>(DataEntity scribeEntity) where T: new()
		{
			//create a new entity from the specified type: 
			var restEntity = new T();

			//get the list of fields from the GoToWebinar entity. Extract into a Dictionary.
			var fieldInfo =
				typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public
				| BindingFlags.GetProperty).ToDictionary(key => key.Name, value => value);

			//create a matching set of key value pairs
			var matchingFieldValues = scribeEntity.Properties.Where(field => fieldInfo.ContainsKey(field.Key));

			//Loop through the field dictionary and assign the values from Scribe Online to the
			//GoToWebinar Entity fields:
			foreach (var field in matchingFieldValues)
			{
				fieldInfo[field.Key].SetValue(restEntity, field.Value, null);
			}

			//values are all assigned to our GoToWebinar entity, hand it back:
			return restEntity;

		}

		private DataEntity CreateRegistrant(Registrant registrant)
		{

			//we can't create a registrant without a webinar key;
			//Do this for individual registrants so that the operation batch can keep running if only 1 or 2 are busted.

			if (string.IsNullOrWhiteSpace(registrant.WebinarKey))
			{
				throw new InvalidOperationException("Unable to create a registrant without a Webinar Key.");
			}

			if (string.IsNullOrWhiteSpace(registrant.Email))
			{
				throw new InvalidOperationException("Email is required");
			}

			if (string.IsNullOrWhiteSpace(registrant.FirstName))
			{
				throw new InvalidOperationException("First Name is required");
			}

			if (string.IsNullOrWhiteSpace(registrant.LastName))
			{
				throw new InvalidOperationException("Last Name is required");
			}

			//otherwise, create it: 
			return this._goToWebinarClient.CreateRegistrant(_connectionInfo["AccessToken"], _connectionInfo["OrganizerKey"],
				registrant.WebinarKey, registrant.FirstName, registrant.LastName, registrant.Email);

		}

	}
}
