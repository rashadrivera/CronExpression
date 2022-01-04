using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CronExpressionTests {

	[TestClass]
	[TestCategory("Days")]
	public class DayOfWeekTests {

		[DataTestMethod]
		[DataRow(DayOfWeek.Sunday, "1/3/2022 03:43:12"/*Mon*/, "01/09/2022 00:00")]
		[DataRow(DayOfWeek.Saturday, "1/5/2022 03:43:12"/*Wed*/, "01/08/2022 00:00")]
		[DataRow(DayOfWeek.Tuesday, "1/5/2022 03:43:12"/*Wed*/, "01/11/2022 00:00")]
		[TestProperty("Type", "Positive")]
		public void ValidDayNextTest(int value, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * * * {value}");

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow(2/*Tue*/, 5/*Fri*/, "1/6/2021 10:59:12"/*Wed*/, "1/6/2021 11:00")]
		[DataRow(2/*Tue*/, 5/*Fri*/, "1/8/2021 23:59:12"/*Fri*/, "1/12/2021 00:00")]
		[TestProperty("Type", "Positive")]
		public void ValidDayRangesTest(int start, int end, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * * * {start}-{end}");
			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow(1/*Mon*/, 2, "1/2/2022 10:59:12"/*Sun*/, "1/3/2022 00:00"/*Mon*/)]
		[DataRow(1/*Mon*/, 2, "1/3/2022 10:59:12"/*Mon*/, "1/3/2022 11:00"/*Mon*/)]
		[DataRow(1/*Mon*/, 2, "1/4/2022 10:59:12"/*Tue*/, "1/5/2022 00:00"/*Wed*/)]
		[TestProperty("Type", "Positive")]
		public void ValidDayStepTest(int start, int step, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * * * {start}/{step}");
			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		//        Sun,Wed,Sat
		//        ^   Tue
		//        ^   ^ Thu-Fri
		//        ^   ^ ^
		[DataRow("0/3,2,4-5", "1/1/2022 23:59:12"/*Sat*/, "1/2/2022 00:00"/*Sun*/)]
		[DataRow("0/3,2,4-5", "1/2/2022 23:59:12"/*Sun*/, "1/4/2022 00:00"/*Tue*/)]
		[DataRow("0/3,2,4-5", "1/4/2022 23:59:12"/*Tue*/, "1/5/2022 00:00"/*Wed*/)]
		[DataRow("0/3,2,4-5", "1/5/2022 23:59:12"/*Wed*/, "1/6/2022 00:00"/*Thu*/)]
		[DataRow("0/3,2,4-5", "1/7/2022 23:59:12"/*Fri*/, "1/8/2022 00:00"/*Sat*/)]
		[TestProperty("Type", "Positive")]
		public void ValidDaySplitsTest(string splits, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression($"* * * * {splits}");

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}
	}
}
