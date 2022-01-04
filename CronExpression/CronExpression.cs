using CronExpression.Internals;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace System {

	// https://crontab.guru/

	public sealed class CronExpression {

		private readonly string _Expression;

		public CronExpression(string expression) {

			_ValidateExpression(expression);

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

			var matches = _GetMatches(this._Expression);

			var _Minute = MinuteCronInterval.Parse(matches.Result("${Minutes}"));
			var _Hour = HourCronInterval.Parse(matches.Result("${Hours}"));
			var _Day = DayCronInterval.Parse(matches.Result("${Days}"));
			var _Month = MonthCronInterval.Parse(matches.Result("${Month}"));
			var _DayOfWeek = DayOfWeekCronInterval.Parse(matches.Result("${DayOfWeek}"));

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

		static void _ValidateExpression(string expression) {

			Debug.Assert(!string.IsNullOrWhiteSpace(nameof(expression)), "Invalid method call");

			if (string.IsNullOrEmpty(expression?.Trim()))
				throw new ArgumentNullException(nameof(expression));

			var match = _GetMatches(expression);

			if (!match.Success)
				throw new ArgumentException("Value is not a valid CRON expression", nameof(expression));

			MinuteCronInterval.Validate(match.Result("${Minutes}"));
			HourCronInterval.Validate(match.Result("${Hours}"));
			DayCronInterval.Validate(match.Result("${Days}"));
			MonthCronInterval.Validate(match.Result("${Month}"));
			DayOfWeekCronInterval.Validate(match.Result("${DayOfWeek}"));
		}

		static Match _GetMatches(string expression)
			=> Regex.Match(
				expression,
				"(?<Minutes>[0-9*,/-]+) (?<Hours>[0-9*,/-]+) (?<Days>[0-9*,/-]+) (?<Month>[0-9*,/\\-a-z]+) (?<DayOfWeek>[0-9*,/\\-a-z]+)",
				RegexOptions.Singleline,
				TimeSpan.FromSeconds(0.2)
			);

		#endregion
	}
}
