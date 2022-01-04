using CronExpression.Internals;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace System {

	// https://crontab.guru/

	public sealed class CronExpression {

		private readonly string _Expression;

		private readonly MinuteParser _Minute;

		private readonly HourParser _Hour;

		private readonly DayParser _Day;

		private readonly MonthParser _Month;

		//private readonly DayOfWeekParser _DayOfWeek;

		public CronExpression(string expression) {

			_ValidateExpression(expression);

			this._Expression = expression;

			var matches = _GetMatches(expression.Trim());

			this._Minute = MinuteParser.Parse(matches.Result("${Minutes}"));
			this._Hour = HourParser.Parse(matches.Result("${Hours}"));
			this._Day = DayParser.Parse(matches.Result("${Days}"));
			this._Month = MonthParser.Parse(matches.Result("${Month}"));
			//this._DayOfWeek = DayOfWeekParser.Parse(matches.Result("${DayOfWeek}"));
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

			var returnValue = this._Month
				.Apply(target);
			returnValue = this._Day
				.Apply(returnValue);
			returnValue = this._Hour
				.Apply(returnValue);
			returnValue = this._Minute
				.Apply(returnValue);

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
