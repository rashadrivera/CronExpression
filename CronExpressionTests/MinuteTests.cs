using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CronExpressionTests {

	[TestClass]
	[TestCategory("Minutes")]
	public class MinuteTests {

		[DataTestMethod]
		[DataRow(10)]
		[TestProperty("Type", "Positive")]
		public void ValidMinuteNextTest(int value) {

			var expression = new CronExpression($"{value} * * * *");

			var now = DateTimeOffset.Now;
			var expected = now + TimeSpan.FromMinutes(value);
			var nextInterval = expression.Next(now);
			Assert.AreEqual(expected, nextInterval);
		}

		[DataTestMethod]
		[DataRow(10)]
		[TestProperty("Type", "Positive")]
		public void ValidMinuteOffsetTest(int value) {

#error Left Off here

			var expression = new CronExpression($"{value} * * * *");

			var expected = TimeSpan.FromMinutes(value);
			var offset = expression.Offset();
			Assert.AreEqual(expected, offset);
		}
	}
}
