using CronExpression.Internals;

namespace System {

	// https://crontab.guru/

	public sealed class CronExpression {

		private readonly string _Expression;

		public CronExpression(string expression) {

			CronExpressionHelper.ValidateExpression(expression);

			this._Expression = expression.Trim();
		}

		public DateTimeOffset Next(DateTimeOffset target) {

			var returnValue = target
				// We check the next minute forward
				.AddMinutes(1);

			var cycle = 0;
			do {
				returnValue = this._Next(returnValue);
				if (++cycle > 30)
					throw new RunawayCronExpressionException(this._Expression);
			} while (!this._IsDateValid(returnValue));

			return returnValue;
		}

		#region Helper Method(s)

		/// <summary>
		/// Compute the next time based on Cron schedule
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		DateTimeOffset _Next(DateTimeOffset target) {

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
		bool _IsDateValid(DateTimeOffset target) {
			var peekNext = this._Next(target);
			var returnValue = peekNext == target;
			return returnValue;
		}

		#endregion
	}
}
