// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlQueryBuilder.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Query;

namespace Scribe.Connector.Cdk.Sample.SYS
{
    /// <summary>
    /// Builds up query statements based on 
    /// a QueryIput or OperationInput.
    /// </summary>
    class SqlQueryBuilder
    {
        private const string SelectKeyword = "SELECT";
        private const string FromKeyword = "FROM";
        private const string DeleteKeyword = "DELETE";
        private const string CreateKeyword = "INSERT";
        private const string UpdateKeyword = "UPDATE";
        private const string WhereKeyword = "WHERE";
        private const string LeftKeyWord = "LEFT";
        private const string JoinKeyword = "JOIN";
        private const string OnKeyword = "ON";
        private const string OrderKeyword = "ORDER BY";
        private const string DescendingKeyword = "DESC";
        private const string AscendingKeyword = "ASC";
        private const string AsKeyword = "AS";
        private const string IntoKeyword = "INTO";
        private const string ValuesKeyword = "VALUES";
        private const string SetKeyword = "SET";
        private const string ConvertKeyword = "CONVERT";
        private const string DateTimeKeyword = "DATETIME";
        private const string IfKeyword = "IF";
        private const string ElseKeyword = "ELSE";
        private const string BeginKeyword = "BEGIN";
        private const string EndKeyword = "END";
        private const string ExistsKeyword = "EXISTS";
        private string _query;

        private readonly DataTable _columnDefinitions;

        /// <summary>
        /// List of child entities and their related foreign keys are kept in this list.
        /// This will be used in the data access layer to check for null values against the related entities;
        /// Key: 'Relationship Name' Value: Comma delimeted list of relationship keys
        /// </summary>
        public Dictionary<string, string> RelatedForeignKeys = new Dictionary<string, string>();

        /// <summary>
        /// Constructor for building a select statement for using in CRUD operations
        /// with a command builder.
        /// </summary>
        /// <param name="operationInput"></param>
        /// <param name="lookupCondition"></param>
        /// <param name="queryType"></param>
        public SqlQueryBuilder(DataEntity operationInput, Expression lookupCondition, Globals.QueryType queryType)
        {
            Parse(operationInput, lookupCondition, queryType);
        }

        public SqlQueryBuilder(DataEntity operationInput, Globals.QueryType queryType)
        {
            Parse(operationInput, new EntityProperties(), queryType);
        }

        public SqlQueryBuilder(DataEntity operationInput, EntityProperties filterColumns, Globals.QueryType queryType)
        {
            Parse(operationInput, filterColumns, queryType);
        }

        public SqlQueryBuilder(Query query, DataTable columnDefinitions)
        {
            _columnDefinitions = columnDefinitions;
            Parse(query);
        }

        public override string ToString()
        {
            return _query;
        }

        #region private

        private void Parse(DataEntity input, EntityProperties filterColumns, Globals.QueryType queryType)
        {
            var query = new StringBuilder();

            if (queryType == Globals.QueryType.Upsert)
            {
                var whereClause = ParseWhereClause(filterColumns);
                query.AppendFormat("{0} {3} ({1}{2})", IfKeyword, ParseQueryType(input, Globals.QueryType.Select), whereClause, ExistsKeyword);
                query.AppendFormat("{0}{1}{0}{5}{6}{0}{3}{0}{4}{0}{1}{0}{2}{0}{3}", Environment.NewLine, BeginKeyword,
                    ParseQueryType(input, Globals.QueryType.Insert),
                    EndKeyword, ElseKeyword, ParseQueryType(input, Globals.QueryType.Update), whereClause);
            }
            else
            {
                query.Append(ParseQueryType(input, queryType));
                query.Append(ParseWhereClause(filterColumns));
            }

            _query = query.ToString();
        }

        /// <summary>
        /// Create a select string from the given table and columns.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="lookupCondition"></param>
        /// <param name="queryType"></param>
        private void Parse(DataEntity input, Expression lookupCondition, Globals.QueryType queryType)
        {
            var query = new StringBuilder(ParseQueryType(input, queryType));

            // add where clause (optional))
            if (lookupCondition != null && queryType != Globals.QueryType.Insert)
            {
                var whereClause = new StringBuilder(" " + WhereKeyword + " ");
                ParseWhereClause(whereClause, lookupCondition);
                query.Append(whereClause);
            }

            _query = query.ToString();
        }

        private void Parse(Query query)
        {
            StringBuilder queryBuilder = new StringBuilder(SelectKeyword);

            //add columns to the query
            queryBuilder.Append(ParseColumns(query.RootEntity));

            //add from clause to the query
            queryBuilder.Append(ParseFromClause(query.RootEntity.ObjectDefinitionFullName));

            //add joins
            if (query.RootEntity.ChildList != null && query.RootEntity.ChildList.Count > 0)
            {
                ParseJoins(query.RootEntity, queryBuilder);
            }

            // add where clause (optional)
            if (query.Constraints != null)
            {
                var whereClause = new StringBuilder(" " + WhereKeyword + " ");
                ParseWhereClause(whereClause, query.Constraints);
                queryBuilder.Append(whereClause);
            }

            //add order by clause
            if (query.RootEntity.SequenceList != null && query.RootEntity.SequenceList.Count > 0)
            {
                queryBuilder.Append(" ");
                queryBuilder.Append(ParseOrderBy(query.RootEntity));
            }

            _query = queryBuilder.ToString();
        }

        private string ParseQueryType(DataEntity input, Globals.QueryType queryType)
        {
            StringBuilder query;

            switch (queryType)
            {
                case Globals.QueryType.Insert:
                    query = new StringBuilder(CreateKeyword);
                    // add into clause
                    query.Append(ParseInsertQuery(input));
                    break;
                case Globals.QueryType.Delete:
                    query = new StringBuilder(DeleteKeyword);
                    // add from clause
                    query.Append(ParseFromClause(input.ObjectDefinitionFullName));
                    break;
                case Globals.QueryType.Update:
                    query = new StringBuilder(UpdateKeyword);
                    //add the set values
                    query.Append(ParseUpdateQuery(input));
                    break;
                case Globals.QueryType.Count:
                    query = new StringBuilder(SelectKeyword);
                    query.Append(" COUNT(*) ");
                    query.Append(ParseFromClause(input.ObjectDefinitionFullName));
                    break;
                case Globals.QueryType.Select:
                default:
                    query = new StringBuilder(SelectKeyword);
                    //retrieve the list of column names
                    List<string> columNames = input.Properties.Select(entityProperty => entityProperty.Key).ToList();
                    // add columns
                    query.Append(ParseColumns(columNames, input.ObjectDefinitionFullName));
                    // add from clause
                    query.Append(ParseFromClause(input.ObjectDefinitionFullName));
                    break;
            }

            return query.ToString();
        }

        /// <summary>
        /// provides the columns in a format to add to the select statement, overload for parsing columns of a data entity input
        /// </summary>
        /// <param name="columnNames"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        private string ParseColumns(List<string> columnNames, string entityName)
        {
            return ParseColumns(columnNames, entityName, string.Empty, false);
        }

        /// <summary>
        /// provides the columns in a format to add to the select statement
        /// </summary>
        /// <param name="columnNames">columns to add</param>
        /// <param name="entityName"></param>
        /// <param name="relationshipName"></param>
        /// <param name="useAlias"></param>
        /// <returns>colums formated fpor a SQL select statement</returns>
        private string ParseColumns(List<string> columnNames, string entityName, string relationshipName, bool useAlias)
        {
            var selectColumns = new StringBuilder();
            var columnPrefix = useAlias ? relationshipName : entityName;

            // add columns
            if (columnNames != null && columnNames.Count > 0)
            {
                foreach (var property in columnNames)
                {
                    selectColumns.Append(" [");
                    selectColumns.Append(columnPrefix);
                    selectColumns.Append("].[");
                    selectColumns.Append(property);

                    //if no relationship is specified we know that this is part of the root entity
                    if (string.IsNullOrWhiteSpace(relationshipName) == false)
                    {
                        var aliasKey = string.Format("{0}.{1}.{2}", relationshipName, entityName, property);
                        selectColumns.Append("] ");
                        selectColumns.Append(AsKeyword);
                        selectColumns.Append(" [");
                        selectColumns.Append(aliasKey);
                    }

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
        /// Provides formated columns for the select statement using the QueryEntity object
        /// </summary>
        /// <param name="queryEntity"></param>
        /// <returns></returns>
        private string ParseColumns(QueryEntity queryEntity)
        {
            var selectedChildColumns = new StringBuilder();
            var selectedParentColumns = new StringBuilder();

            var relationshipName = queryEntity.ParentQueryEntity != null ? queryEntity.Name : string.Empty;

            if (queryEntity.ChildList != null && queryEntity.ChildList.Count > 0)
            {
                //Append the columns of the child entities
                foreach (var relatedQueryEntity in queryEntity.ChildList)
                {
                    //add the column to the query
                    selectedChildColumns.Append(",");
                    selectedChildColumns.Append(ParseColumns(relatedQueryEntity));
                }
            }

            // if self join use the Name for an alias or the same table joined more than once.
            var useAlias = false;
            if (queryEntity.ParentQueryEntity != null)
            {
                useAlias = queryEntity.ObjectDefinitionFullName == queryEntity.ParentQueryEntity.ObjectDefinitionFullName ||
                           queryEntity.ParentQueryEntity.ChildList.Where(child => child.ObjectDefinitionFullName == queryEntity.ObjectDefinitionFullName).Count() > 1;
            }

            //Append the query properties 
            selectedParentColumns.Append(ParseColumns(queryEntity.PropertyList, queryEntity.ObjectDefinitionFullName, relationshipName, useAlias));

            selectedParentColumns.Append(selectedChildColumns.ToString());

            return selectedParentColumns.ToString();
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
                    columnValues.Append("null,");
                }
                else
                {
                    //create a the right comparison value
                    ComparisonValue rightValue = new ComparisonValue(ComparisonValueType.Constant, property.Value);

                    columnValues.Append(GetRightFormattedComparisonValue(property.Key, rightValue));
                    columnValues.Append(",");
                }
            }

            //remove the trailing commas and replace them with a closing brace
            columnNames = columnNames.Replace(',', ')', columnNames.Length - 1, 1);
            columnValues = columnValues.Replace(',', ')', columnValues.Length - 1, 1);

            return string.Format(" {0} [{1}] {2} {3}{4} {5}", IntoKeyword,
                input.ObjectDefinitionFullName,
                columnNames, Environment.NewLine, ValuesKeyword, columnValues);
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
                string rightValue;

                if (property.Value == null)
                {
                    rightValue = "NULL";
                }
                else
                {
                    //create a new comparison value for the column value
                    var value = new ComparisonValue(ComparisonValueType.Constant, property.Value);
                    rightValue = GetRightFormattedComparisonValue(property.Key, value);
                }


                updateValues.Append(string.Format("[{0}] = {1},", property.Key, rightValue));
            }

            //remove the trailing comma
            updateValues = updateValues.Remove(updateValues.Length - 1, 1);

            return string.Format(" [{0}] " + SetKeyword + " {1}", input.ObjectDefinitionFullName, updateValues);
        }

        private string ParseFromClause(string objectName)
        {
            var fromClause = new StringBuilder();

            fromClause.Append(" ");
            fromClause.Append(FromKeyword);
            fromClause.Append(" [");
            fromClause.Append(objectName);
            fromClause.Append("]");

            return fromClause.ToString();
        }

        private static string ParseWhereClause(EntityProperties properties)
        {
            var whereClause = new StringBuilder();
            if (properties != null && properties.Count > 0)
            {
                whereClause.AppendFormat(" {0} ", WhereKeyword);

                int index = 1;
                foreach (var property in properties)
                {
                    if (property.Value == null)
                    {
                        whereClause.AppendFormat("[{0}] IS NULL", property.Key);
                    }
                    else
                    {
                        var rightComparisonValue = new ComparisonValue(ComparisonValueType.Constant, property.Value);

                        whereClause.AppendFormat("[{0}] = {1}", property.Key,
                                                 GetRightFormattedComparisonValue(rightComparisonValue));
                    }

                    if (index != properties.Count)
                    {
                        whereClause.Append(" AND ");
                    }

                    index++;
                }
            }

            return whereClause.ToString();
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

                        //change the operator if a null value is referenced
                        //valid queries may allow [Column Name] = NULL
                        ParseNullOperators(comparisonExpression);

                        expressionBuilder.AppendFormat(" {0} ", ParseComparisionOperator(comparisonExpression.Operator));

                        if (OperatorHasRightValue(comparisonExpression.Operator))
                        {
                            expressionBuilder.Append(GetRightFormattedComparisonValue(comparisonExpression.LeftValue, comparisonExpression.RightValue));
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
                                whereClause.Append(" AND ");
                                break;
                            case LogicalOperator.Or:
                                whereClause.Append(" OR ");
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
            var formattedComparisonValue = new StringBuilder();

            //Check if value is of type Property, which would indicate a reference to a column name in sql
            if (comparisonValue.ValueType == ComparisonValueType.Property)
            {
                //split property hierarchy
                var comparisonHierarchy = comparisonValue.Value.ToString().Split('.');
                //The format for the incomming data is 'TableName.ColumnName'
                //Split the properties appart and create a 'formal' data link resulting in [TableName].[ColumnName]
                foreach (string t in comparisonHierarchy)
                {
                    formattedComparisonValue.Append("[");
                    formattedComparisonValue.Append(t);
                    formattedComparisonValue.Append("].");
                }
                formattedComparisonValue.Remove(formattedComparisonValue.Length - 1, 1);
            }
            else
            {
                // Values that are constant need to be enclosed in single quotes.
                formattedComparisonValue.Append(
                    string.Format(comparisonValue.ValueType == ComparisonValueType.Constant ? "'{0}'" : "{0}",
                                  comparisonValue.Value));
            }

            return formattedComparisonValue.ToString();
        }

        /// <summary>
        /// Gets the comparison value formatted for use in a sql statement.
        /// This is used for query execution
        /// </summary>
        /// <param name="columnName">name of the column used in the comparison</param>
        /// <param name="rightValue">value to be formatted in the comparison</param>
        /// <returns>The value formatted for use in a sql statement.</returns>
        private string GetRightFormattedComparisonValue(string columnName, ComparisonValue rightValue)
        {
            string rightFormattedValue = string.Empty;

            //Determine whether or not column definitions have been implemented, 
            //if they havn't then we know that the query is requested from the ExecuteOpertion method
            if (_columnDefinitions == null)
            {
                rightFormattedValue = GetRightFormattedComparisonValue(rightValue);
            }
            else
            {
                //create a new comparison value for the left side of the statement using the column name
                ComparisonValue leftValue = new ComparisonValue();
                leftValue.ValueType = ComparisonValueType.Property;
                leftValue.Value = columnName;
                rightFormattedValue = GetRightFormattedComparisonValue(leftValue, rightValue);
            }

            return rightFormattedValue;
        }

        /// <summary>
        /// Gets the comparison value formatted for use in a sql statement.
        /// This is used for query execution
        /// </summary>
        /// <param name="leftValue"></param>
        /// <param name="rightValue"></param>
        /// <returns>
        /// The value formatted for use in a sql statement.
        /// </returns>
        private string GetRightFormattedComparisonValue(ComparisonValue leftValue, ComparisonValue rightValue)
        {
            object comparisonValue;

            //check if the left value is a property, which in this case would be a column name
            if (leftValue.ValueType == ComparisonValueType.Property && _columnDefinitions != null)
            {
                //retrieve the name of the column from the right value.
                //The Incomming format is [Column Name]
                //use the datatypes stored in the column definitions to propery convert the data stored in the right value
                comparisonValue = DataTypeConverter.ToSqlValue(leftValue.Value.ToString().Split('.').Last(), rightValue.Value, _columnDefinitions);
            }
            else
            {
                comparisonValue = rightValue.Value;
            }

            bool valueIsDate = (comparisonValue is DateTime);
            string value = comparisonValue.ToString();
            string result;

            if (valueIsDate)
            {
                DateTime dateTimeValue = ((DateTime)(comparisonValue));

                if (dateTimeValue.Kind != DateTimeKind.Utc)
                {
                    dateTimeValue = dateTimeValue.ToUniversalTime();
                }

                value = dateTimeValue.ToString("s");

                result = string.Format("{0}({1}, '{2}')", ConvertKeyword, DateTimeKeyword, value);
            }
            else
            {
                result = string.Format(rightValue.ValueType == ComparisonValueType.Constant ? "'{0}'" : "{0}", value);
            }

            return result;
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
                result = string.Format(comparisonValue.ValueType == ComparisonValueType.Constant ? "'{0}'" : "{0}", value.Replace("'", "''"));
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
                case ComparisonOperator.NotLike:
                    comparisonString = "NOT LIKE";
                    break;
                case ComparisonOperator.NotEqual:
                    comparisonString = "<>";
                    break;
                default:
                    throw new NotSupportedException(string.Format("The comparison operator {0} is not supported.", comparisonOperator));
            }

            return comparisonString;
        }

        /// <summary>
        /// Constructs the order by SQL syntax for a root entity.
        /// </summary>
        /// <param name="queryEntity"></param>
        /// <returns></returns>
        private StringBuilder ParseOrderBy(QueryEntity queryEntity)
        {
            var orderbyClause = new StringBuilder(OrderKeyword);

            foreach (var sequence in queryEntity.SequenceList)
            {
                orderbyClause.AppendFormat(" [{0}].[{1}] {2},", queryEntity.ObjectDefinitionFullName, sequence.PropertyName, sequence.Direction == SequenceDirection.Descending ? DescendingKeyword : AscendingKeyword);
            }

            // remove traling ","
            orderbyClause.Remove(orderbyClause.Length - 1, 1);

            return orderbyClause;
        }

        /// <summary>
        /// Parse query joins
        /// </summary>
        /// <param name="queryEntity"></param>
        /// <param name="joinClause">currently build join clause</param>
        private void ParseJoins(QueryEntity queryEntity, StringBuilder joinClause)
        {
            var useAlias = false;
            //loop through each of the child entities
            foreach (var relatedQueryEntity in queryEntity.ChildList)
            {
                //verify that the query entity has a parent relation present
                if (relatedQueryEntity.RelationshipToParent != null)
                {
                    joinClause.Append(" ");
                    joinClause.Append(LeftKeyWord);
                    joinClause.Append(" ");
                    joinClause.Append(JoinKeyword);
                    joinClause.Append(" [");
                    joinClause.Append(relatedQueryEntity.ObjectDefinitionFullName);
                    joinClause.Append("] ");
                    // add alias if this is a self join or this table is joined more than once
                    if (relatedQueryEntity.ObjectDefinitionFullName == relatedQueryEntity.ParentQueryEntity.ObjectDefinitionFullName ||
                        queryEntity.ChildList.Where(child => child.ObjectDefinitionFullName == relatedQueryEntity.ObjectDefinitionFullName).Count() > 1)
                    {
                        useAlias = true;
                        joinClause.Append(AsKeyword);
                        joinClause.Append(" [");
                        joinClause.Append(relatedQueryEntity.Name);
                        joinClause.Append("] ");
                    }
                    joinClause.Append(OnKeyword);
                    joinClause.Append(ParseJoinProperties(relatedQueryEntity, useAlias));

                    //add relationship keys to the Foreign Key list
                    RelatedForeignKeys.Add(relatedQueryEntity.Name,
                                           relatedQueryEntity.RelationshipToParent.ChildProperties);
                }
            }
        }

        /// <summary>
        /// Parse the properties that are required for the join
        /// </summary>
        /// <param name="queryEntity"></param>
        /// <param name="useAlias"></param>
        /// <returns></returns>
        private string ParseJoinProperties(QueryEntity queryEntity, bool useAlias)
        {
            var joinQueryProperties = new StringBuilder();

            //convert the csv of parent and child properties to lists
            var parentProperties = queryEntity.RelationshipToParent.ParentProperties.Split(',').ToList();
            var childProperties = queryEntity.RelationshipToParent.ChildProperties.Split(',').ToList();

            // do we need an alias for a self join or the same table joined more than once?
            var childObjectName = useAlias ? queryEntity.Name : queryEntity.ObjectDefinitionFullName;

            //add each of the properties to the join
            for (var i = 0; i < parentProperties.Count; i++)
            {
                if (i > 0)
                {
                    joinQueryProperties.Append("AND");
                }

                joinQueryProperties.Append(" [");
                joinQueryProperties.Append(childObjectName);
                joinQueryProperties.Append("].[");
                joinQueryProperties.Append(childProperties[i]);
                joinQueryProperties.Append("] = [");
                joinQueryProperties.Append(queryEntity.ParentQueryEntity.ObjectDefinitionFullName);
                joinQueryProperties.Append("].[");
                joinQueryProperties.Append(parentProperties[i]);
                joinQueryProperties.Append("] ");
            }

            return joinQueryProperties.ToString();
        }

        /// <summary>
        /// Check if the expression is searing for a null value or is a null object and change the operator to handle this appropriatly
        /// </summary>
        /// <param name="comparisonExpression"></param>
        private void ParseNullOperators(ComparisonExpression comparisonExpression)
        {
            //check for any form of a null right expression
            if (comparisonExpression.RightValue == null ||
                comparisonExpression.RightValue.Value == null)
            {
                //set the appropriate operator
                switch (comparisonExpression.Operator)
                {
                    case ComparisonOperator.Equal:
                        comparisonExpression.Operator = ComparisonOperator.IsNull;
                        break;
                    case ComparisonOperator.NotEqual:
                        comparisonExpression.Operator = ComparisonOperator.IsNotNull;
                        break;
                    case ComparisonOperator.IsNull:
                    case ComparisonOperator.IsNotNull:
                        break;
                    default:
                        throw new NotSupportedException(string.Format(ErrorCodes.NullOperatorNotValid.Description, comparisonExpression.Operator));

                }
            }
        }
        #endregion
    }
}
