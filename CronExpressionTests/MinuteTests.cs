using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CronExpressionTests {

	[TestClass]
	[TestCategory("Minutes")]
	public class MinuteTests {

		[DataTestMethod]
		[DataRow(0, "12/26/2021 03:43:12", "12/26/2021 04:00")]
		[DataRow(10, "12/26/2021 03:43:12", "12/26/2021 04:10")]
		[DataRow(59, "12/26/2021 03:43:12", "12/26/2021 03:59")]
		[TestProperty("Type", "Positive")]
		public void ValidMinuteNextTest(int value, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new CronExpression($"{value} * * * *");

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[TestMethod]
		[TestProperty("Type", "Positive")]
		public void ValidMinuteAsStarTest() {

			var expression = new CronExpression($"* * * * *");

			var now = DateTimeOffset.Now;
			var expected = now
				.AddSeconds(-now.Second)
				.AddMilliseconds(-now.Millisecond)
				.AddMinutes(1);
			var nextInterval = expression.Next(now);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow(0, 10, "12/30/2021 10:55:12", "12/30/2021 11:00")]
		[DataRow(0, 59, "12/30/2021 10:55:12", "12/30/2021 10:56")]
		[DataRow(55, 56, "12/30/2021 10:55:12", "12/30/2021 10:56")]
		[DataRow(55, 55, "12/30/2021 10:55:12", "12/30/2021 10:56")]
		[DataRow(56, 59, "12/30/2021 10:55:12", "12/30/2021 10:56")]
		[TestProperty("Type", "Positive")]
		public void ValidMinuteRangesTest(int start, int end, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new CronExpression($"{start}-{end} * * * *");
			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow(0, 10, "12/30/2021 10:13:12", "12/30/2021 10:20")]
		[DataRow(0, 3, "12/30/2021 10:13:12", "12/30/2021 10:15")]
		[DataRow(13, 5, "12/30/2021 10:13:12", "12/30/2021 10:13")]
		[DataRow(13, 5, "12/30/2021 10:10:12", "12/30/2021 10:13")]
		[DataRow(13, 5, "12/30/2021 10:14:12", "12/30/2021 10:15")]
		[TestProperty("Type", "Positive")]
		public void ValidMinuteStepTest(int start, int step, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new CronExpression($"{start}/{step} * * * *");
			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}
	}
}
