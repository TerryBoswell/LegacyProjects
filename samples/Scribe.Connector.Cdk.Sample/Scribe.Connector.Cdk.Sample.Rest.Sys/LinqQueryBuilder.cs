// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinqQueryBuilder.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2013 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Query;
using System.Globalization;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys
{
	/// <summary>
	/// Extension methods to create Linq expression from the Scribe Online Query
	/// </summary>
	public static class LinqQueryBuilder
	{

		#region Public Methods

		/// <summary>
		/// Returns a Linq expression based on the query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public static string ToLinqExpression(this Query query)
		{

			var whereClause = new StringBuilder();
			if (query.Constraints != null)
			{
				//Create a string 'Where' clause based on the user's query
				ParseWhereClause(whereClause, query.Constraints);
			}

			return whereClause.ToString();

		}

		/// <summary>
		/// Creates an 'Order By' string expression from the query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public static string ToOrderByLinqExpression(this Query query)
		{
			var orderByStatement = string.Empty;
			var queryEntity = query.RootEntity;
			var orderByBuilder = new StringBuilder();

			if (queryEntity != null && queryEntity.SequenceList.Count > 0)
			{
				
				//Add each of the sequences into our list: 
				foreach (var sequence in queryEntity.SequenceList)
				{
				
					orderByBuilder.Append(string.Format("{0} {1}, ",
						sequence.PropertyName, sequence.Direction.ToString()));

				}

				//trim the last 2 characters off the end (the , and a space): 
				orderByStatement = orderByBuilder.ToString().Substring(
					0, orderByBuilder.ToString().Length - 2);
				
			}

			return orderByStatement;

		}
	
		#endregion

		#region Private Methods

		/// <summary>
		/// Creates a string 'where' clause based on the Scribe Online Expression
		/// </summary>
		/// <param name="whereClause"></param>
		/// <param name="lookupCondition"></param>
		private static void ParseWhereClause(StringBuilder whereClause, Expression lookupCondition)
		{

			if (lookupCondition == null)
			{
				return;
			}

			//build the correct linq expression based on the query.
			//We're only supporting Comparison and Logical queries in this connector
			switch (lookupCondition.ExpressionType)
			{
				
				case ExpressionType.Comparison:
					var comparisonExpression = lookupCondition as ComparisonExpression;
					var comparisonBuilder = new StringBuilder();
					if (comparisonExpression == null)
					{
						throw new InvalidOperationException("This isn't a valid operation.");
					}

					//Handle the comparison operator inside this query: 
					switch (comparisonExpression.Operator)
					{
							
						case ComparisonOperator.Equal:
						case ComparisonOperator.Greater:
						case ComparisonOperator.GreaterOrEqual:
						case ComparisonOperator.IsNotNull:
						case ComparisonOperator.IsNull:
						case ComparisonOperator.Less:
						case ComparisonOperator.LessOrEqual:
						case ComparisonOperator.NotEqual:
							
							comparisonBuilder.Append(GetLeftFormattedComparisonValue(comparisonExpression.LeftValue));

							//Check for a NULL comparison and change the operator if needed:
							ParseNullOperators(comparisonExpression);

							//Add the operator to the string: 
							comparisonBuilder.AppendFormat(" {0} ", ParseOperator(comparisonExpression.Operator));

							//Add the right side of the expression if it exists:
							comparisonBuilder.Append(
								OperatorHasRightValue(comparisonExpression.Operator)
								? GetRightFormattedComparisonValue(comparisonExpression.RightValue)
								: "null");
							break;

						case ComparisonOperator.Like:
							comparisonBuilder.Append(BuildLike(comparisonExpression));
							break;
							
						case ComparisonOperator.NotLike:
							comparisonBuilder.Append(string.Format("!{0}", BuildLike(comparisonExpression)));
							break;

						default:
							throw new NotSupportedException("Operation not supported");
					}

					//append the text to the incoming where clause: 
					whereClause.Append(comparisonBuilder.ToString());
					break;
				
				case ExpressionType.Logical:
					var logicalExpression = lookupCondition as LogicalExpression;

					if (logicalExpression == null)
					{
						throw new InvalidOperationException("This isn't a valid operation");
					}

					//Recursively loop through these until we're down to 'comparison' operators: 
					ParseWhereClause(whereClause, logicalExpression.LeftExpression);

					//Append the correct operator:
					switch (logicalExpression.Operator)
					{

						case LogicalOperator.And:
							whereClause.Append(" && ");
							break;
						
						case LogicalOperator.Or:
							whereClause.Append(" || ");
							break;
						
						default:
							throw new NotSupportedException(string.Format("Logical operator {0} not supported", logicalExpression.Operator.ToString()));

					}

					//Recursively parse through the right expression
					ParseWhereClause(whereClause, logicalExpression.RightExpression);

					break;
				
				default:
					break;
			}
		}

		/// <summary>
		/// Creates a regex based on the left and right values in the ComparisonExpression for a 'Is Like' type comparison
		/// </summary>
		/// <param name="comparisonExpression"></param>
		/// <returns></returns>
		private static string BuildLike(ComparisonExpression comparisonExpression)
		{
			const string format = "Regex.IsMatch({0} != null ? {0} :\"\", {1}, RegexOptions.IgnoreCase)";

			string returnString = string.Format(format, comparisonExpression.LeftValue.Value.ToString().Split('.')[1],
				Quote(string.Format("^{0}$",
				comparisonExpression.RightValue.Value.ToString().Replace("%", ".*"))));

			return returnString;

		}

		/// <summary>
		/// Flips the comparison operator if the query operator is null
		/// </summary>
		/// <param name="comparisonExpression"></param>
		private static void ParseNullOperators(ComparisonExpression comparisonExpression)
		{
			if (comparisonExpression.RightValue == null ||
					comparisonExpression.RightValue.Value == null)
			{
				switch (comparisonExpression.Operator)
				{
					case ComparisonOperator.Equal:
						comparisonExpression.Operator = ComparisonOperator.IsNull;
						break;

					case ComparisonOperator.NotEqual:
						comparisonExpression.Operator = ComparisonOperator.IsNotNull;
						break;

					case ComparisonOperator.IsNotNull:
					case ComparisonOperator.IsNull:
						break;
					
					default:
						throw new NotSupportedException("This operation is not supported");
				}
			}
		}

		/// <summary>
		/// Returns the correct math operator based on the comparison operator
		/// </summary>
		/// <param name="comparisonOperator"></param>
		/// <returns></returns>
		private static string ParseOperator(ComparisonOperator @comparisonOperator)
		{
			string operation;

			switch (@comparisonOperator)
			{

				case ComparisonOperator.Greater:
					operation = ">";
					break;

				case ComparisonOperator.GreaterOrEqual:
					operation = ">=";
					break;

				case ComparisonOperator.NotEqual:
				case ComparisonOperator.IsNotNull:
					operation = "!=";
					break;

				case ComparisonOperator.Equal:
				case ComparisonOperator.IsNull:
					operation = "==";
					break;

				case ComparisonOperator.Less:
					operation = "<";
					break;

				case ComparisonOperator.LessOrEqual:
					operation = "<=";
					break;

				default:
					throw new NotSupportedException("Operation is not supported");
			}

			return operation;

		}

		/// <summary>
		/// Determines if the comparison operator has a right value
		/// </summary>
		/// <param name="comparisonOperator"></param>
		/// <returns></returns>
		private static bool OperatorHasRightValue(ComparisonOperator @comparisonOperator)
		{
			var isLeft = @comparisonOperator == ComparisonOperator.IsNull || @comparisonOperator == ComparisonOperator.IsNotNull;

			return !isLeft;

		}

		/// <summary>
		/// Formats the right comparison value and hands it back
		/// </summary>
		/// <param name="comparisonValue"></param>
		/// <returns></returns>
		private static string GetRightFormattedComparisonValue(ComparisonValue comparisonValue)
		{

			var isValueDate = (comparisonValue.Value is DateTime);
			var value = Convert.ToString(comparisonValue.Value, CultureInfo.InvariantCulture);
			string result;

			if (isValueDate)
			{
				//result = comparisonValue.Value.ToString();
				var dateTimeValue = ((DateTime)(comparisonValue.Value));
				value = dateTimeValue.ToString("o");

				result = string.Format(
					"DateTime.Parse({0}, null, DateTimeStyles.RoundtripKind)",
					Quote(string.Format("{0}", value)));
				
			}
			else if (comparisonValue.ValueType == ComparisonValueType.Constant)
			{
				//if it's a constant value type and a string, wrap it in quotes: 
				result = comparisonValue.Value is string
					? string.Format("{0}", Quote(value))
					: value;
			}
			else
			{
				//otherwise, just return the raw value:
				result = value;
			}

			return result;

		}

		/// <summary>
		/// Formats the left comparison value and hands it back. 
		/// </summary>
		/// <param name="comparisonValue"></param>
		/// <returns></returns>
		private static string GetLeftFormattedComparisonValue(ComparisonValue comparisonValue)
		{
			var formattedValue = new StringBuilder();

			if (comparisonValue.ValueType == ComparisonValueType.Property)
			{
				var propertyParts = comparisonValue.Value.ToString().Split('.');
				var propertyName = propertyParts[propertyParts.Length - 1];

				formattedValue.AppendFormat("{0}", propertyName);
			}
			else
			{
				//if the value is constant, wrap it in quotes, otherwise, just append it.
				formattedValue.Append(
					string.Format(comparisonValue.ValueType == ComparisonValueType.Constant ? Quote("{0}") : "{0}",
					comparisonValue.Value));
			}

			return formattedValue.ToString();

		}

		/// <summary>
		/// Wraps the incoming value in quotes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static string Quote(string value)
		{
			return string.Format("\"{0}\"", value);
		}

		#endregion

	}
}
