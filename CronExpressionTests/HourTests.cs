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

		//[TestMethod]
		//[TestProperty("Type", "Positive")]
		//public void ValidHourAsStarTest() {

		//	var expression = new System.CronExpression($"* * * * *");

		//	var now = DateTimeOffset.Now;
		//	var expected = now
		//		.AddSeconds(-now.Second)
		//		.AddMilliseconds(-now.Millisecond)
		//		.AddHours(1);
		//	var nextInterval = expression.Next(now);
		//	Assert.AreEqual(expected, nextInterval);
		//}

		//[DataTestMethod]
		//[DataRow(0, 10, "12/30/2021 10:55:12", "12/30/2021 11:00")]
		//[DataRow(0, 59, "12/30/2021 10:55:12", "12/30/2021 10:56")]
		//[DataRow(55, 56, "12/30/2021 10:55:12", "12/30/2021 10:56")]
		//[DataRow(55, 55, "12/30/2021 10:55:12", "12/30/2021 10:56")]
		//[DataRow(56, 59, "12/30/2021 10:55:12", "12/30/2021 10:56")]
		//[TestProperty("Type", "Positive")]
		//public void ValidHourRangesTest(int start, int end, string targetAsString, string expectedAsString) {

		//	var target = DateTimeOffset.Parse(targetAsString);
		//	var expected = DateTimeOffset.Parse(expectedAsString);
		//	var expression = new System.CronExpression($"{start}-{end} * * * *");
		//	var nextInterval = expression.Next(target);
		//	Assert.AreEqual(expected, nextInterval);
		//}

		//[DataTestMethod]
		//[DataRow(0, 10, "12/30/2021 10:13:12", "12/30/2021 10:20")]
		//[DataRow(0, 3, "12/30/2021 10:13:12", "12/30/2021 10:15")]
		//[DataRow(13, 5, "12/30/2021 10:13:12", "12/30/2021 10:18")]
		//[DataRow(13, 5, "12/30/2021 10:10:12", "12/30/2021 10:13")]
		//[DataRow(13, 5, "12/30/2021 10:17:12", "12/30/2021 10:18")]
		//[DataRow(13, 5, "12/30/2021 10:18:12", "12/30/2021 10:23")]
		//[TestProperty("Type", "Positive")]
		//public void ValidHourStepTest(int start, int step, string targetAsString, string expectedAsString) {

		//	var target = DateTimeOffset.Parse(targetAsString);
		//	var expected = DateTimeOffset.Parse(expectedAsString);
		//	var expression = new System.CronExpression($"{start}/{step} * * * *");
		//	var nextInterval = expression.Next(target);
		//	Assert.AreEqual(expected, nextInterval);
		//}

		//[DataTestMethod]
		//[DataRow("5,13,35/3", "12/26/2021 03:43:12", "12/26/2021 03:44")]
		//[DataRow("5,13,35/3", "12/26/2021 03:05:12", "12/26/2021 03:13")]
		//[DataRow("5,13,35/3", "12/26/2021 03:05:12", "12/26/2021 03:13")]
		//[DataRow("5,13,35/3", "12/26/2021 03:13:13", "12/26/2021 03:35")]
		//[DataRow("5,13,35/3", "12/26/2021 03:34:34", "12/26/2021 03:35")]
		//[DataRow("5,13,35/3", "12/26/2021 03:35:35", "12/26/2021 03:38")]
		//[TestProperty("Type", "Positive")]
		//public void ValidHourSplitsTest(string splits, string targetAsString, string expectedAsString) {

		//	var target = DateTimeOffset.Parse(targetAsString);
		//	var expected = DateTimeOffset.Parse(expectedAsString);
		//	var expression = new System.CronExpression($"{splits} * * * *");

		//	var nextInterval = expression.Next(target);
		//	Assert.AreEqual(expected, nextInterval);
		//}
	}
}
