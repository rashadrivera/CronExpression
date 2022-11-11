using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CronExpressionTests {

	[TestClass]
	[TestCategory("Hours")]
	public class HourTests {

		[DataTestMethod]
		[DataRow(0, "12/26/2021 03:43:12", "12/27/2021 00:00")]
		[DataRow(10, "12/26/2021 03:43:12", "12/26/2021 10:00")]
		[DataRow(23, "12/26/2021 03:43:12", "12/26/2021 23:00")]
		[TestProperty("Type", "Positive")]
		public void ValidHourNextTest(int value, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* {value} * * *");

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow(0, 10, "12/30/2021 10:59:12", "12/31/2021 00:00")]
		[DataRow(6, 7, "12/30/2021 10:59:12", "12/31/2021 06:00")]
		[DataRow(22, 23, "12/30/2021 10:59:12", "12/30/2021 22:00")]
		[TestProperty("Type", "Positive")]
		public void ValidHourRangesTest(int start, int end, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* {start}-{end} * * *");
			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow(0, 10, "12/30/2021 08:13:12 -05:00", "12/30/2021 10:00 -05:00")]
		[DataRow(7, 7, "12/30/2021 00:13:12 -05:00", "12/30/2021 07:00 -05:00")]
		[DataRow(7, 7, "12/30/2021 07:59:12 -05:00", "12/30/2021 14:00 -05:00")]
		[DataRow(23, 1, "12/30/2021 07:59:12 -05:00", "12/30/2021 23:00 -05:00")]
		[DataRow(23, 1, "12/30/2021 23:59:12 -05:00", "12/31/2021 23:00 -05:00")]
		[TestProperty("Type", "Positive")]
		public void ValidHourStepTest(int start, int step, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* {start}/{step} * * *");
			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow("2,5,7/7", "12/26/2021 00:43:12 -05:00", "12/26/2021 02:00 -05:00")]
		[DataRow("2,5,7/7", "12/26/2021 03:05:12 -05:00", "12/26/2021 05:00 -05:00")]
		[DataRow("2,5,7/7", "12/26/2021 06:05:12 -05:00", "12/26/2021 07:00 -05:00")]
		[DataRow("2,5,7/7", "12/26/2021 08:13:13 -05:00", "12/26/2021 14:00 -05:00")]
		[DataRow("2,5,7/7", "12/26/2021 15:34:34 -05:00", "12/26/2021 21:00 -05:00")]

		[DataRow("2,5,7/7", "12/26/2021 22:35:35 -05:00", "12/27/2021 02:00 -05:00")]
		[TestProperty("Type", "Positive")]
		public void ValidHourSplitsTest(string splits, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* {splits} * * *");

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}
	}
}
