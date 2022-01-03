
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CronExpressionTests {

	[TestClass]
	[TestCategory("Days")]
	public class DayTests {

		[DataTestMethod]
		// 1 - 5
		[DataRow(1, "12/26/2021 03:43:12", "01/01/2022 00:00")]
		[DataRow(10, "12/26/2021 03:43:12", "01/10/2022 00:00")]
		[DataRow(23, "12/26/2021 03:43:12", "01/23/2022 00:00")]
		[DataRow(30, "01/31/2022 03:43:12", "03/30/2022 00:00 -05:00")]
		[TestProperty("Type", "Positive")]
		public void ValidDayNextTest(int value, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * {value} * *");

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		// 1 - 5
		[DataRow(10, 31, "1/5/2021 10:59:12", "1/10/2021 00:00")]
		[DataRow(10, 31, "1/10/2021 10:04:12", "1/10/2021 10:05")]
		[DataRow(10, 31, "1/31/2021 23:59:00", "2/10/2021 00:00")]
		[DataRow(30, 31, "1/31/2021 23:59:00", "3/30/2021 00:00 -05:00")]
		[DataRow(30, 31, "3/30/2021 23:59:00 -05:00", "3/31/2021 00:00 -05:00")]

		// 6 - 10
		[DataRow(15, 20, "3/30/2021 23:59:00 -05:00", "4/15/2021 00:00 -05:00")]
		[DataRow(15, 20, "4/20/2021 23:59:00 -05:00", "5/15/2021 00:00 -05:00")]
		[TestProperty("Type", "Positive")]
		public void ValidDayRangesTest(int start, int end, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * {start}-{end} * *");
			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow(3, 9, "12/31/2021 08:13:12", "1/3/2022 00:00")]
		[DataRow(3, 9, "1/4/2021 08:13:12", "1/12/2021 00:00")]
		[DataRow(3, 9, "1/31/2021 08:13:12", "2/3/2021 00:00")]
		[DataRow(3, 9, "2/27/2021 08:13:12", "3/3/2021 00:00")]
		[TestProperty("Type", "Positive")]
		public void ValidDayStepTest(int start, int step, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * {start}/{step} * *");
			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		//[DataTestMethod]
		//[DataRow("2,5,7/7", "12/26/2021 00:43:12", "12/26/2021 02:00")]
		//[DataRow("2,5,7/7", "12/26/2021 03:05:12", "12/26/2021 05:00")]
		//[DataRow("2,5,7/7", "12/26/2021 06:05:12", "12/26/2021 07:00")]
		//[DataRow("2,5,7/7", "12/26/2021 08:13:13", "12/26/2021 14:00")]
		//[DataRow("2,5,7/7", "12/26/2021 15:34:34", "12/26/2021 21:00")]
		//[DataRow("2,5,7/7", "12/26/2021 22:35:35", "12/27/2021 02:00")]
		//[TestProperty("Type", "Positive")]
		//public void ValidDaySplitsTest(string splits, string targetAsString, string expectedAsString) {

		//	var target = DateTimeOffset.Parse(targetAsString);
		//	var expected = DateTimeOffset.Parse(expectedAsString);
		//	var expression = new System.CronExpression($"* {splits} * * *");

		//	var nextInterval = expression.Next(target);
		//	Assert.AreEqual(expected, nextInterval);
		//}
	}
}
