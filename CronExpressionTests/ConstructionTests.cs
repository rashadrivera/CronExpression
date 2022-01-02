using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CronExpressionTests {

	[TestClass]
	[TestCategory("Construction")]
	public class ConstructionTests {

		[TestMethod]
		[TestProperty("Type", "Positive")]
		public void DefaultConstructor()
			=> new System.CronExpression("* * * * *");

		[TestMethod]
		[TestProperty("Type", "Negative")]
		public void NullConstructionTest() {

			bool hasFailed = false;
			try {
				new System.CronExpression(null);
			} catch {
				hasFailed = true;
			}

			Assert.IsTrue(hasFailed, $"Value '' should not be permitted");
		}

		[TestMethod]
		[TestProperty("Type", "Negative")]
		public void EmptyStringConstructionTest() {

			bool hasFailed = false;
			try {
				new System.CronExpression(string.Empty);
			} catch {
				hasFailed = true;
			}

			Assert.IsTrue(hasFailed, $"Value '' should not be permitted");
		}

		[DataTestMethod]
		[DataRow(" ")]
		[DataRow("*")]
		[DataRow("* *")]
		[DataRow("* * *")]
		[DataRow("* * * *")]
		[DataRow("-1 * * * *")]
		[DataRow(", * * * *")]
		[DataRow("- * * * *")]
		[DataRow("/ * * * *")]
		[DataRow("60 * * * *")]
		[DataRow("100 * * * *")]
		[TestProperty("Type", "Negative")]
		public void BadExpressionConstructionTest(string value) {

			bool hasFailed = false;
			try {
				new System.CronExpression(value);
			} catch {
				hasFailed = true;
			}

			Assert.IsTrue(hasFailed, $"Value '{value}' should not be permitted");
		}

		[DataTestMethod]
		[DataRow("10 * * * *")]
		[DataRow("10-15 * * * *")]
		[DataRow("1/3 * * * *")]
		[DataRow("1/3,0,1-5 * * * *")]
		[TestProperty("Type", "Positive")]
		public void ValidMinuteExpressionsConstructionTest(string value)
			=> new System.CronExpression(value);

		[DataTestMethod]
		//[DataRow("* 10 * * *")]
		//[DataRow("* 10-15 * * *")]
		//[DataRow("* 1/3 * * *")]
		[DataRow("* 1/3,0,1-5 * * *")]
		[TestProperty("Type", "Positive")]
		public void ValidHourExpressionsConstructionTest(string value)
			=> new System.CronExpression(value);
	}
}
