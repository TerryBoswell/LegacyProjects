using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using Scribe.Core.ConnectorApi.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace Scribe.Connector.etouches
{
    static class DataTableQueryBuilder
    {
        public static string ToSelectExpression(this Query query)
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

            //build the correct datatable select expression based on the query.
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
                            comparisonBuilder.Append(string.Format("NOT {0}", BuildLike(comparisonExpression)));
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
                            whereClause.Append(" AND ");
                            break;

                        case LogicalOperator.Or:
                            whereClause.Append(" OR ");
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
                    operation = "<>";
                    break;

                case ComparisonOperator.Equal:
                case ComparisonOperator.IsNull:
                    operation = "=";
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

                formattedValue.AppendFormat("[{0}]", propertyName.Replace("'","''"));
            }
            else
            {
                //if the value is constant, wrap it in quotes, otherwise, just append it.
                formattedValue.Append(
                    string.Format(comparisonValue.ValueType == ComparisonValueType.Constant ? QuoteSingle("{0}") : "{0}",
                    comparisonValue.Value));
            }

            return formattedValue.ToString();

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
                value = dateTimeValue.ToString("o");  //TODO: Convert to the proper date format

                result = QuoteSingle(value);

            }
            else if (comparisonValue.ValueType == ComparisonValueType.Constant)
            {
                //if it's a constant value type and a string, wrap it in quotes: 
                result = comparisonValue.Value is string
                    ? string.Format("{0}", QuoteSingle(value))
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
        /// Creates a regex based on the left and right values in the ComparisonExpression for a 'Is Like' type comparison
        /// </summary>
        /// <param name="comparisonExpression"></param>
        /// <returns></returns>
        private static string BuildLike(ComparisonExpression comparisonExpression)
        {
            const string format = "[{0}] LIKE {1}";

            string returnString = string.Format(format, comparisonExpression.LeftValue.Value.ToString().Split('.')[1],
                QuoteSingle(comparisonExpression.RightValue.Value.ToString()));

            return returnString;

        }

        /// <summary>
        /// Escapes a text value for usage in a LIKE clause.
        /// </summary>
        /// <param name="valueWithoutWildcards"></param>
        /// <returns></returns>
        private static string EscapeLikeValue(string valueWithoutWildcards)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < valueWithoutWildcards.Length; i++)
            {
                char c = valueWithoutWildcards[i];
                if (c == '*' || c == '%' || c == '[' || c == ']')
                    sb.Append("[").Append(c).Append("]");
                else if (c == '\'')
                    sb.Append("''");
                else
                    sb.Append(c);
            }
            return sb.ToString();
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

        /// <summary>
        /// Wraps the incoming value in single quotes (ticks)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string QuoteSingle(string value)
        {
            return string.Format("'{0}'", value);
        }

        public static IEnumerable<DataEntity> ToDataEntities(this DataRow[] rows, string entityName, List<IPropertyDefinition> definition)
        {
            Dictionary<string, string> propertyDefinitionTypes = new Dictionary<string, string>();
            definition.ForEach(def => {
                propertyDefinitionTypes.Add(def.Name, def.PresentationType);
            });
            var list = new List<DataEntity>();
            foreach (DataRow row in rows)
            {
                var entity = new DataEntity(entityName)
                {
                    Properties = new EntityProperties()
                };
                foreach (DataColumn col in row.Table.Columns)
                {
                    var value = row[col];

                    string type = string.Empty;
                    if (propertyDefinitionTypes.ContainsKey(col.ColumnName))
                    {
                        type = propertyDefinitionTypes[col.ColumnName];
                    }
                    DateTime dt;
                    if (type == "System.DateTime" && DateTime.TryParse(value.ToString(), out dt))
                        entity.Properties.Add(col.ColumnName, dt);
                    else
                        entity.Properties.Add(col.ColumnName, value);
                }
                list.Add(entity);
            }
            return list;
        }

        public static DataEntity FirstDataEntity(this DataRow[] rows, string entityName)
        {
            if (rows.Length == 0)
                return null;

            var row = rows[0];
            var entity = new DataEntity(entityName)
            {
                Properties = new EntityProperties()
            };
            foreach (DataColumn col in row.Table.Columns)
            {
                entity.Properties.Add(col.ColumnName, row[col]);
            }
            return entity;
        }
    }
}
