using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CronExpressionTests {

	[TestClass]
	public sealed class PracticalApplicationTests {

		const string MinuteAndHour_Cron = "3,15-20,35/5 10-15,20,22 * * *";
		[DataTestMethod]
		// 1 - 5
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:01:12", "12/26/2021 11:03:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:03:12", "12/26/2021 11:15:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:15:12", "12/26/2021 11:16:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:37:12", "12/26/2021 11:40:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:59:12", "12/26/2021 12:03:00")]

		// 6 - 10
		[DataRow(MinuteAndHour_Cron, "12/26/2021 02:01:12", "12/26/2021 10:03:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 15:55:12", "12/26/2021 20:03:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 20:59:12", "12/26/2021 22:03:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 22:55:12", "12/26/2021 22:03:00")]

		[TestProperty("Type", "Positive")]
		public void MinutesAndHourTest(string value, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression(value);

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}
	}
}
