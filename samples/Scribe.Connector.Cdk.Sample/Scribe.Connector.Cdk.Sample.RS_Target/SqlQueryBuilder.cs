// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlQueryBuilder.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Query;

namespace Scribe.Connector.Cdk.Sample.RS_Target
{
    /// <summary>
    /// Builds up query statements based on 
    /// a QueryIput or OperationInput.
    /// </summary>
    class SqlQueryBuilder
    {
        private const string SelectKeyword = "select";
        private const string FromKeyword = "from";
        private const string DeleteKeyword = "delete";
        private const string CreateKeyword = "insert";
        private const string UpdateKeyword = "update";

        private string _query;

        /// <summary>
        /// Constructor for building a select statement for using in CRUD operations
        /// with a command builder.
        /// </summary>
        /// <param name="operationInput"></param>
        /// <param name="lookupCondition"></param>
        /// <param name="operationType"></param>
        public SqlQueryBuilder(DataEntity operationInput, Expression lookupCondition, Globals.OperationType operationType)
        {
            Parse(operationInput, lookupCondition, operationType);
        }

        public override string ToString()
        {
            return _query;
        }

        #region private

        /// <summary>
        /// Create a select string from the given table and columns.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="lookupCondition"></param>
        /// <param name="operationType"></param>
        private void Parse(DataEntity input, Expression lookupCondition, Globals.OperationType operationType)
        {
            StringBuilder query;

            switch (operationType)
            {
                case Globals.OperationType.Create:
                    query = new StringBuilder(CreateKeyword);
                    // add into clause
                    query.Append(ParseInsertQuery(input));
                    break;
                case Globals.OperationType.Delete:
                    query = new StringBuilder(DeleteKeyword);
                    // add from clause
                    query.Append(ParseFromClause(input));
                    break;
                case  Globals.OperationType.Update:
                    query = new StringBuilder(UpdateKeyword);
                    //add the set values
                    query.Append(ParseUpdateQuery(input));
                    break;
                default:
                    query = new StringBuilder(SelectKeyword);
                    // add columns
                    query.Append(ParseColumns(input.Properties));
                    // add from clause
                    query.Append(ParseFromClause(input));
                    break;
            }

            // add where clause (optional))
            if (lookupCondition != null)
            {
                var whereClause = new StringBuilder(" where ");
                ParseWhereClause(whereClause, lookupCondition);
                query.Append(whereClause);
            }

            _query = query.ToString();
        }

        /// <summary>
        /// provides the columns in a format to add to the select statement
        /// </summary>
        /// <param name="properties">columns to add</param>
        /// <returns>colums formated fpor a SQL select statement</returns>
        private string ParseColumns(EntityProperties properties)
        {
            var selectColumns = new StringBuilder();

            // add columns
            if (properties != null && properties.Count > 0)
            {
                foreach (var property in properties)
                {
                    selectColumns.Append(" [");
                    selectColumns.Append(property.Key);
                    selectColumns.Append("],");
                }
                selectColumns.Remove(selectColumns.Length - 1, 1);
            }
            else // all columns
            {
                selectColumns.Append(" *");
            }

            return selectColumns.ToString();
        }

        /// <summary>
        /// Provide the query for insert data by parsing the input DataEntity
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ParseInsertQuery(DataEntity input)
        {
            StringBuilder columnNames = new StringBuilder();
            StringBuilder columnValues = new StringBuilder();
            columnNames.Append("(");
            columnValues.Append("(");
            
            // parse through the column names and values 
            foreach (KeyValuePair<string, object> property in input.Properties)
            {
                columnNames.Append(string.Format(" [{0}],", property.Key));
                if (property.Value == null)
                {
                    columnValues.Append("'',");
                }
                else
                {
                    //create a new comparison value for the column value
                    ComparisonValue value = new ComparisonValue(ComparisonValueType.Constant, property.Value);
                    columnValues.Append(GetRightFormattedComparisonValue(value));
                    columnValues.Append(",");
                }
            }

            //remove the trailing commas and replace them with a closing brace
            columnNames = columnNames.Replace(',',')', columnNames.Length - 1,1);
            columnValues = columnValues.Replace(',', ')', columnValues.Length - 1, 1);

            return string.Format(" into [{0}] {1} {2}values {3}", 
                input.ObjectDefinitionFullName,
                columnNames, Environment.NewLine, columnValues);
        }

        /// <summary>
        /// Provide the query for an update by parsing the input DataEntity.
        /// This however does not include the where clause
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ParseUpdateQuery(DataEntity input)
        {
            StringBuilder updateValues = new StringBuilder();
            // parse through the column names and values 
            foreach (var property in input.Properties)
            {
                //create a new comparison value for the column value
                ComparisonValue value = new ComparisonValue(ComparisonValueType.Constant, property.Value);
                updateValues.Append(string.Format("[{0}]={1},", property.Key, GetRightFormattedComparisonValue(value)));
            }

            //remove the trailing comma
            updateValues = updateValues.Remove(updateValues.Length - 1, 1);

            return string.Format(" [{0}] set {1}", input.ObjectDefinitionFullName, updateValues);
        }

        private string ParseFromClause(DataEntity input)
        {
            var fromClause = new StringBuilder();

            fromClause.Append(" ");
            fromClause.Append(FromKeyword);
            fromClause.Append(" [");
            fromClause.Append(input.ObjectDefinitionFullName);
            fromClause.Append("]");

            return fromClause.ToString();
        }

        /// <summary>
        /// parses a lookup condition to produce a sql where clause. 
        /// </summary>
        /// <param name="whereClause">used to store the sql where clause</param>
        /// <param name="lookupCondition">the expression to filter on</param>
        private void ParseWhereClause(StringBuilder whereClause, Expression lookupCondition)
        {
            if (lookupCondition != null)
            {
                whereClause.Append("(");

                switch (lookupCondition.ExpressionType)
                {
                    case ExpressionType.Comparison:
                        var comparisonExpression = lookupCondition as ComparisonExpression;

                        //validate
                        if (comparisonExpression == null)
                        {
                            throw new InvalidOperationException("Invalid Comparision Expression");
                        }
                        // build up the expression
                        var expressionBuilder = new StringBuilder();

                        expressionBuilder.Append(GetLeftFormattedComparisonValue(comparisonExpression.LeftValue));

                        expressionBuilder.AppendFormat(" {0} ", ParseComparisionOperator(comparisonExpression.Operator));

                        if (OperatorHasRightValue(comparisonExpression.Operator))
                        {
                            expressionBuilder.Append(GetRightFormattedComparisonValue(comparisonExpression.RightValue));
                        }

                        var sqlFormattedExpression = expressionBuilder.ToString();

                        whereClause.Append(sqlFormattedExpression);
                        break;

                    case ExpressionType.Logical:
                        var logicalExpression = lookupCondition as LogicalExpression;

                        if (logicalExpression == null)
                        {
                            throw new InvalidOperationException("Invalid Logical Expression");
                        }

                        ParseWhereClause(whereClause, logicalExpression.LeftExpression);

                        switch (logicalExpression.Operator)
                        {
                            case LogicalOperator.And:
                                whereClause.Append(" and ");
                                break;
                            case LogicalOperator.Or:
                                whereClause.Append(" or ");
                                break;
                            default:
                                throw new NotSupportedException(string.Format("UNSUPPORTED LOGICAL OPERATION: {0}", logicalExpression.Operator));
                        }

                        ParseWhereClause(whereClause, logicalExpression.RightExpression);
                        break;
                }

                whereClause.Append(")");
            }

            return;
        }

        /// <summary>
        /// test for a value to the right of the operator 
        /// </summary>
        /// <param name="operator"></param>
        /// <returns></returns>
        private bool OperatorHasRightValue(ComparisonOperator @operator)
        {
            var onlyLeft = @operator == ComparisonOperator.IsNull || @operator == ComparisonOperator.IsNotNull;

            return !onlyLeft;
        }

        /// <summary>
        /// Gets the comparison value formatted for use in a sql statement.
        /// </summary>
        /// <param name="comparisonValue">
        /// The comparison value.
        /// </param>
        /// <returns>
        /// The value formatted for use in a sql statement.
        /// </returns>
        private static string GetLeftFormattedComparisonValue(ComparisonValue comparisonValue)
        {
            // Values that are constant need to be enclosed in single quotes.
            var formattedComparisonValue = string.Format(comparisonValue.ValueType == ComparisonValueType.Constant ? "[{0}]" : "{0}", comparisonValue.Value);

            return formattedComparisonValue;
        }

        /// <summary>
        /// Gets the comparison value formatted for use in a sql statement.
        /// </summary>
        /// <param name="comparisonValue">
        /// The comparison value.
        /// </param>
        /// <returns>
        /// The value formatted for use in a sql statement.
        /// </returns>
        private static string GetRightFormattedComparisonValue(ComparisonValue comparisonValue)
        {
            bool valueIsDate = (comparisonValue.Value.GetType() == typeof(DateTime));
            string value = comparisonValue.Value.ToString();
            string result;

            if (valueIsDate)
            {
                DateTime dateTimeValue = ((DateTime)(comparisonValue.Value));

                value = dateTimeValue.ToString("s");

                result = string.Format("convert(datetime, '{0}')", value);
            }
            else
            {
                result = string.Format(comparisonValue.ValueType == ComparisonValueType.Constant ? "'{0}'" : "{0}", value.Replace("'","''"));
            }

            return result;
        }

        /// <summary>
        /// Parse through the expression to convert it to a query
        /// </summary>
        /// <param name="comparisonOperator"></param>
        /// <returns></returns>
        private static string ParseComparisionOperator(ComparisonOperator comparisonOperator)
        {
            string comparisonString;

            switch (comparisonOperator)
            {
                case ComparisonOperator.Equal:
                    comparisonString = "=";
                    break;
                case ComparisonOperator.Greater:
                    comparisonString = ">";
                    break;
                case ComparisonOperator.GreaterOrEqual:
                    comparisonString = ">=";
                    break;
                case ComparisonOperator.IsNotNull:
                    comparisonString = "IS NOT NULL";
                    break;
                case ComparisonOperator.IsNull:
                    comparisonString = "IS NULL";
                    break;
                case ComparisonOperator.Less:
                    comparisonString = "<";
                    break;
                case ComparisonOperator.LessOrEqual:
                    comparisonString = "<=";
                    break;
                case ComparisonOperator.Like:
                    comparisonString = "LIKE";
                    break;
                case ComparisonOperator.NotEqual:
                    comparisonString = "<>";
                    break;
                default:
                    throw new NotSupportedException(string.Format("The comparison operator {0} is not supported.", comparisonOperator));
            }

            return comparisonString;
        }
        #endregion
    }
}
