using CronExpression.Internals;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace System {

	// https://crontab.guru/

	public sealed class CronExpression {

		private readonly MinuteParser _Minute;

		private readonly HourParser _Hour;

		//private readonly DaysParser _Day;

		//private readonly MonthParser _Month;

		//private readonly DayOfWeekParser _DayOfWeek;

		public CronExpression(string expression) {

			_ValidateExpression(expression);

			var matches = _GetMatches(expression.Trim());

			this._Minute = MinuteParser.Parse(matches.Result("${Minutes}"));
			this._Hour = HourParser.Parse(matches.Result("${Hours}"));
			//this._Day = DayParser.Parse(matches.Result("${Days}"));
			//this._Month = MonthParser.Parse(matches.Result("${Month}"));
			//this._DayOfWeek = DayOfWeekParser.Parse(matches.Result("${DayOfWeek}"));
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

		public DateTimeOffset Next(DateTimeOffset target) {

			var targetMinusSecondsDown = target
				.AddSeconds(-target.Second)
				.AddMilliseconds(-target.Millisecond);

			var returnValue = this._Hour
				.Apply(targetMinusSecondsDown);
			returnValue = this._Minute
				.Apply(returnValue);

			if (returnValue == targetMinusSecondsDown)
				return returnValue.AddMinutes(1);
			return returnValue;
		}

		#region Member Class(es)

		//sealed class HoursParser {

		//	readonly string _MinutePart;

		//	public HoursParser(string minutePart) {

		//		if (string.IsNullOrEmpty(minutePart))
		//			throw new ArgumentNullException(nameof(minutePart));
		//		if (!Validate(minutePart))
		//			throw new ArgumentException("Value is not valid", nameof(minutePart));
		//		this._MinutePart = minutePart;
		//		this.IsValid = false;
		//	}

		//	public bool IsValid { get; }

		//	public static bool Validate(string expression)
		//		=> false;
		//}

		//sealed class DaysParser {

		//	readonly string _MinutePart;

		//	public DaysParser(string minutePart) {

		//		if (string.IsNullOrEmpty(minutePart))
		//			throw new ArgumentNullException(nameof(minutePart));
		//		if (!Validate(minutePart))
		//			throw new ArgumentException("Value is not valid", nameof(minutePart));
		//		this._MinutePart = minutePart;
		//		this.IsValid = false;
		//	}

		//	public bool IsValid { get; }

		//	public static bool Validate(string expression)
		//		=> false;
		//}

		//sealed class MonthParser {

		//	readonly string _MinutePart;

		//	public MonthParser(string minutePart) {

		//		if (string.IsNullOrEmpty(minutePart))
		//			throw new ArgumentNullException(nameof(minutePart));
		//		if (!Validate(minutePart))
		//			throw new ArgumentException("Value is not valid", nameof(minutePart));
		//		this._MinutePart = minutePart;
		//		this.IsValid = false;
		//	}

		//	public bool IsValid { get; }

		//	public static bool Validate(string expression)
		//		=> false;
		//}

		//sealed class DayOfWeekParser {

		//	readonly string _MinutePart;

		//	public DayOfWeekParser(string minutePart) {

		//		if (string.IsNullOrEmpty(minutePart))
		//			throw new ArgumentNullException(nameof(minutePart));
		//		if (!Validate(minutePart))
		//			throw new ArgumentException("Value is not valid", nameof(minutePart));
		//		this._MinutePart = minutePart;
		//		this.IsValid = false;
		//	}

		//	public bool IsValid { get; }

		//	public static bool Validate(string expression)
		//		=> false;
		//}

		#endregion
	}
}
