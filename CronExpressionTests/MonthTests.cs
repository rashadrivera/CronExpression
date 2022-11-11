using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CronExpressionTests {

	[TestClass]
	[TestCategory("Month")]
	public class MonthTests {

		[DataTestMethod]
		[DataRow(1, "12/26/2021 03:43:12 -05:00", "01/01/2022 00:00 -05:00")]
		[DataRow(10, "12/26/2021 03:43:12 -05:00", "10/01/2022 00:00 -05:00")]
		[DataRow(12, "12/26/2021 03:43:12 -05:00", "12/26/2021 03:44:00 -05:00")]
		[TestProperty("Type", "Positive")]
		public void ValidMonthNextTest(int value, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * * {value} *");

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}


		[DataTestMethod]
		[DataRow(1, 3, "12/30/2021 10:55:12 -05:00", "1/1/2022 00:00 -05:00")]
		[DataRow(1, 3, "4/20/2022 10:55:12 -05:00", "1/1/2023 00:00 -05:00")]
		[DataRow(7, 11, "1/27/2022 10:55:12 -05:00", "7/1/2022 00:00 -05:00")]
		[DataRow(7, 8, "8/31/2022 23:59:12 -05:00", "7/1/2023 00:00 -05:00")]
		[TestProperty("Type", "Positive")]
		public void ValidMonthRangesTest(int start, int end, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * * {start}-{end} *");
			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow(3, 3, "1/30/2022 10:13:12 -05:00", "3/1/2022 00:00 -05:00")]
		[DataRow(3, 3, "11/10/2021 10:13:12 -05:00", "12/1/2021 00:00 -05:00")]
		[DataRow(2, 5, "3/10/2021 10:13:12 -05:00", "7/1/2021 00:00 -05:00")]
		[DataRow(2, 5, "7/31/2021 23:59:00 -05:00", "12/1/2021 00:00 -05:00")]
		[TestProperty("Type", "Positive")]
		public void ValidMonthStepTest(int start, int step, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * * {start}/{step} *");
			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow("5,7,9/2", "1/26/2021 03:42:12 -05:00", "5/1/2021 00:00 -05:00")]
		[DataRow("5,7,9/2", "5/31/2021 23:59:00 -05:00", "7/1/2021 00:00 -05:00")]
		[DataRow("5,7,9/2", "7/31/2021 23:59:00 -05:00", "9/1/2021 00:00 -05:00")]
		[DataRow("5,7,9/2", "9/30/2021 23:59:00 -05:00", "11/1/2021 00:00 -05:00")]
		[DataRow("5,7,9/2", "11/30/2021 23:59:00 -05:00", "5/1/2022 00:00 -05:00")]
		[TestProperty("Type", "Positive")]
		public void ValidMonthSplitsTest(string splits, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * * {splits} *");

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}
	}
}
