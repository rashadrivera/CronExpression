﻿using CronExpression.Internals;

namespace System {

	// https://crontab.guru/

	public sealed class CronExpression {

		private readonly string _Expression;

		public CronExpression(string expression) {

			CronExpressionHelper.ValidateExpression(expression);

			this._Expression = expression.Trim();
		}

		public DateTimeOffset Next(DateTimeOffset target)
			=> this._Next(
				target
					// We check the next minute forward
					.AddMinutes(1)
			);

		public bool IsWithinSchedule(DateTimeOffset target)
			=> this._IsWithinSchedule(target);

		public TimeSpan Timeout(DateTimeOffset target) {

			if (IsWithinSchedule(target))
				return TimeSpan.Zero;

			var next = this._Next(target);
			return next - target;
		}

		#region Helper Method(s)

		DateTimeOffset _Next(DateTimeOffset target) {

			var returnValue = target;
			var timeout = DateTime.Now.AddSeconds(0.2);
			do {
				returnValue = this._Project(returnValue);
				if (timeout < DateTime.Now)
					throw new RunawayCronExpressionException(this._Expression);
			} while (!this._IsWithinSchedule(returnValue));

			return returnValue;
		}

		/// <summary>
		/// Compute the next time based on Cron schedule
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		DateTimeOffset _Project(DateTimeOffset target) {

			var parts = CronExpressionHelper.ExtractParts(this._Expression);

			var _Minute = new MinuteCronInterval(parts.Minutes);
			var _Hour = new HourCronInterval(parts.Hours);
			var _Day = new DayCronInterval(parts.Days);
			var _Month = new MonthCronInterval(parts.Months);
			var _DayOfWeek = new DayOfWeekCronInterval(parts.DayOfWeek);

			var returnValue = target;
			returnValue = _DayOfWeek.ApplyInterval(returnValue);
			returnValue = _Month.ApplyInterval(returnValue);
			returnValue = _Day.ApplyInterval(returnValue);
			returnValue = _Hour.ApplyInterval(returnValue);
			returnValue = _Minute.ApplyInterval(returnValue);

			return returnValue;
		}

		/// <summary>
		/// The target is valid so long as it does not need adjustment per
		/// the Cron schedule
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		bool _IsWithinSchedule(DateTimeOffset target) {
			var peekNext = this._Project(target);
			var returnValue = peekNext == target;
			return returnValue;
		}

		#endregion
	}
}
