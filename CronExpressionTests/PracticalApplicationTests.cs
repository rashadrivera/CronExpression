using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CronExpressionTests {

	[TestClass]
	public sealed class PracticalApplicationTests {

		#region MinuteAndHour

		const string MinuteAndHour_Cron = "3,15-20,35/5 10-15,20,22 * * *";

		[DataTestMethod]
		//// 1 - 5
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:01:12", "12/26/2021 11:03:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:03:12", "12/26/2021 11:15:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:15:12", "12/26/2021 11:16:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:37:12", "12/26/2021 11:40:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:59:12", "12/26/2021 12:03:00")]

		//// 6 - 10
		[DataRow(MinuteAndHour_Cron, "12/26/2021 20:59:12", "12/26/2021 22:03:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 02:01:12", "12/26/2021 10:03:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 15:55:12", "12/26/2021 20:03:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 22:55:12", "12/27/2021 10:03:00")]

		[TestProperty("Type", "Positive")]
		public void MinutesAndHourTest(string value, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression(value);

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		#endregion

		#region MinutesThroughMonth

		const string MinuteThroughMonth_Cron = "1 1 4/5 2,8 *";

		[DataTestMethod]
		[DataRow(MinuteThroughMonth_Cron, "12/26/2021 11:01:12 -05:00", "2/4/2022 01:01:00 -05:00")]
		[DataRow(MinuteThroughMonth_Cron, "2/5/2021 11:01:12 -05:00", "2/9/2021 01:01:00 -05:00")]
		[DataRow(MinuteThroughMonth_Cron, "3/1/2021 11:01:12 -05:00", "8/4/2021 01:01:00 -05:00")]
		[DataRow(MinuteThroughMonth_Cron, "9/4/2022 01:01:00 -05:00", "2/4/2023 01:01:00")]
		[TestProperty("Type", "Positive")]
		public void MinutesThroughMonthTest(string value, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression(value);

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}

		#endregion

		const string AllParts_Cron = "1 2 3 4 6,2-3";// 6 = Sat, 2 = Tue, 3 = Wed

		[DataTestMethod]
		[DataRow(AllParts_Cron, "1/1/2022 00:00:00 -05:00", "4/3/2024 02:01 -05:00")]
		[DataRow(AllParts_Cron, "4/3/2024 02:01 -05:00", "4/3/2027 02:01 -05:00")]
		[TestProperty("Type", "Positive")]
		public void AllPartsTest(string value, string targetAsString, string expectedAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expected = DateTimeOffset.Parse(expectedAsString);
			var expression = new System.CronExpression(value);

			var nextInterval = expression.Next(target);
			Assert.AreEqual(expected, nextInterval);
		}


		[DataTestMethod]
		[DataRow(AllParts_Cron, "4/3/2024 02:01 -05:00")]
		[DataRow(AllParts_Cron, "4/3/2027 02:01 -05:00")]
		[DataRow(MinuteThroughMonth_Cron, "2/4/2022 01:01:00 -05:00")]
		[DataRow(MinuteThroughMonth_Cron, "2/4/2023 01:01:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:03:00")]
		[DataRow(MinuteAndHour_Cron, "12/26/2021 11:15:00")]
		[TestProperty("Type", "Positive")]
		public void IsWithinScheduleTests(string value, string targetAsString) {

			var target = DateTimeOffset.Parse(targetAsString);
			var expression = new System.CronExpression(value);
			Assert.IsTrue(expression.IsWithinSchedule(target));
		}
	}
}
