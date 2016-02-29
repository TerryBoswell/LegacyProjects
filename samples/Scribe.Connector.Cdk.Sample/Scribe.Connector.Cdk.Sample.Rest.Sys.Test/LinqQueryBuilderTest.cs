using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Core.ConnectorApi.Query;
using Scribe.Core.ConnectorApi;

namespace Scribe.Connector.Cdk.Sample.Rest.Sys.Test
{
	
	/// <summary>
	/// Tests to show the usage of the LinqQueryBuilder extension methods
	/// </summary>
	[TestClass]
	public class LinqQueryBuilderTest
	{

		[TestMethod]
		public void LinqQuery_ComparisonQueryTest()
		{

			//Arrange
			var query = BuildComparisonQuery();

			//Act
			string result = query.ToLinqExpression();

			//Assert
			Assert.AreEqual("Name == Bobby", result);

		}

		[TestMethod]
		public void LinqQuery_LogicalQueryTest()
		{
			//Arrange
			var query = BuildLogicalQuery();

			//Act 
			string result = query.ToLinqExpression();

			//Assert 
			Assert.AreEqual("CreditMax > 5000 && Profit > 100 && Revenue > 100 && CreditMin > 100",
				result);
		}

		[TestMethod]
		public void LinqQuery_LogicalAndOrTest()
		{
			//Arrange
			var query = BuildLogicalMixedQuery();

			//Act
			string result = query.ToLinqExpression();

			//Assert:
			Assert.AreEqual("CreditMax > 5000 && Profit > 100 && Revenue > 100 || CreditMin > 100",
				result);

		}

		[TestMethod]
		public void LinqQuery_OrderByTest()
		{
			//Arrange
			var query = BuildComparisonQuery();

			//Act:
			string result = query.ToOrderByLinqExpression();

			//Assert: 
			Assert.AreEqual("Name Ascending, Address Descending", result);

		}


		#region private helpers

		private Query BuildComparisonQuery()
		{

			var sequence1 = new Sequence
			{
				PropertyName = "Name",
				Direction = SequenceDirection.Ascending
			};

			var sequence2 = new Sequence
			{
				PropertyName = "Address",
				Direction  = SequenceDirection.Descending
			};

			var rootEntity = new QueryEntity
			{
				SequenceList = new List<Sequence>()
			};

			rootEntity.SequenceList.Add(sequence1);
			rootEntity.SequenceList.Add(sequence2);

			var constraints = new ComparisonExpression
			{
				Operator = ComparisonOperator.Equal,
				ExpressionType = ExpressionType.Comparison,
				LeftValue = new ComparisonValue { Value = "Name", ValueType = ComparisonValueType.Property },
				RightValue = new ComparisonValue { Value = "Bobby", ValueType = ComparisonValueType.Property }
			};
			
			var query = new Query
			{
				Constraints = constraints,
				RootEntity = rootEntity
			};


			return query;

		}

		private Query BuildLogicalQuery()
		{
			LogicalExpression logicalExpression1 = new LogicalExpression();
			logicalExpression1.Operator = LogicalOperator.And;

			//Top left
			LogicalExpression leftExpression1 = new LogicalExpression();
			leftExpression1.Operator = LogicalOperator.And;
			ComparisonExpression comparisonLeft1 = new ComparisonExpression();
			comparisonLeft1.ExpressionType = ExpressionType.Comparison;
			comparisonLeft1.Operator = ComparisonOperator.Greater;
			comparisonLeft1.LeftValue = new ComparisonValue { Value = "Account.CreditMax" };
			comparisonLeft1.RightValue = new ComparisonValue { Value = 5000 };

			//nested in left
			ComparisonExpression comparisonRight1 = new ComparisonExpression();
			comparisonRight1.ExpressionType = ExpressionType.Comparison;
			comparisonRight1.Operator = ComparisonOperator.Greater;
			comparisonRight1.LeftValue = new ComparisonValue { Value = "Account.Profit" };
			comparisonRight1.RightValue = new ComparisonValue { Value = 100 };

			leftExpression1.LeftExpression = comparisonLeft1;
			leftExpression1.RightExpression = comparisonRight1;

			//Top Right
			LogicalExpression rightExpression = new LogicalExpression();
			rightExpression.Operator = LogicalOperator.And;
			ComparisonExpression comparisonLeft2 = new ComparisonExpression();
			comparisonLeft2.ExpressionType = ExpressionType.Comparison;
			comparisonLeft2.Operator = ComparisonOperator.Greater;
			comparisonLeft2.LeftValue = new ComparisonValue { Value = "Account.Revenue" };
			comparisonLeft2.RightValue = new ComparisonValue { Value = 100 };

			//nested in right
			ComparisonExpression comparisonRight2 = new ComparisonExpression();
			comparisonRight2.ExpressionType = ExpressionType.Comparison;
			comparisonRight2.Operator = ComparisonOperator.Greater;
			comparisonRight2.LeftValue = new ComparisonValue { Value = "Account.CreditMin" };
			comparisonRight2.RightValue = new ComparisonValue { Value = 100 };

			rightExpression.LeftExpression = comparisonLeft2;
			rightExpression.RightExpression = comparisonRight2;

			logicalExpression1.LeftExpression = leftExpression1;
			logicalExpression1.RightExpression = rightExpression;

			var query = new Query
			{
				Constraints = logicalExpression1,
			};

			return query;

		}

		private Query BuildLogicalMixedQuery()
		{
			LogicalExpression logicalExpression1 = new LogicalExpression();
			logicalExpression1.Operator = LogicalOperator.And;

			//Top left -- AND --
			LogicalExpression leftExpression1 = new LogicalExpression();
			leftExpression1.Operator = LogicalOperator.And;
			ComparisonExpression comparisonLeft1 = new ComparisonExpression();
			comparisonLeft1.ExpressionType = ExpressionType.Comparison;
			comparisonLeft1.Operator = ComparisonOperator.Greater;
			comparisonLeft1.LeftValue = new ComparisonValue { Value = "Account.CreditMax" };
			comparisonLeft1.RightValue = new ComparisonValue { Value = 5000 };

			ComparisonExpression comparisonRight1 = new ComparisonExpression();
			comparisonRight1.ExpressionType = ExpressionType.Comparison;
			comparisonRight1.Operator = ComparisonOperator.Greater;
			comparisonRight1.LeftValue = new ComparisonValue { Value = "Account.Profit" };
			comparisonRight1.RightValue = new ComparisonValue { Value = 100 };

			leftExpression1.LeftExpression = comparisonLeft1;
			leftExpression1.RightExpression = comparisonRight1;

			//Top Right  -- OR --
			LogicalExpression rightExpression = new LogicalExpression();
			rightExpression.Operator = LogicalOperator.Or;
			ComparisonExpression comparisonLeft2 = new ComparisonExpression();
			comparisonLeft2.ExpressionType = ExpressionType.Comparison;
			comparisonLeft2.Operator = ComparisonOperator.Greater;
			comparisonLeft2.LeftValue = new ComparisonValue { Value = "Account.Revenue" };
			comparisonLeft2.RightValue = new ComparisonValue { Value = 100 };

			ComparisonExpression comparisonRight2 = new ComparisonExpression();
			comparisonRight2.ExpressionType = ExpressionType.Comparison;
			comparisonRight2.Operator = ComparisonOperator.Greater;
			comparisonRight2.LeftValue = new ComparisonValue { Value = "Account.CreditMin" };
			comparisonRight2.RightValue = new ComparisonValue { Value = 100 };

			rightExpression.LeftExpression = comparisonLeft2;
			rightExpression.RightExpression = comparisonRight2;

			logicalExpression1.LeftExpression = leftExpression1;
			logicalExpression1.RightExpression = rightExpression;

			var query = new Query
			{
				Constraints = logicalExpression1
			};

			return query;

		}


		#endregion


	}
}
