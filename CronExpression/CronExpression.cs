using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace System {

	// https://crontab.guru/

	public sealed class CronExpression {

		private readonly MinuteParser[] _Minutes;

		private readonly HoursParser[] _Hours;

		private readonly DaysParser[] _Days;

		private readonly MonthParser[] _Month;

		private readonly DayOfWeekParser[] _DayOfWeek;

		public CronExpression(string expression) {

			_ValidateExpression(expression);

			var matches = _GetMatches(expression.Trim());

			this._Minutes = MinuteParser.Parse(matches.Result("${Minutes}"));
			//this._Hours = new HoursParser(matches.Result("${Hours}"));
			//this._Days = new DaysParser(matches.Result("${Days}"));
			//this._Month = new MonthParser(matches.Result("${Month}"));
			//this._DayOfWeek = new DayOfWeekParser(matches.Result("${DayOfWeek}"));
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

		public DateTimeOffset Next()
			=> throw new NotImplementedException();

		public DateTimeOffset NextInterval(int offset)
			=> throw new NotImplementedException();

		public TimeSpan Offset()
			=> throw new NotImplementedException();

		public TimeSpan OffsetInterval(int offset)
			=> throw new NotImplementedException();

		#region Member Class(es)

		sealed class MinuteParser {

			readonly IBaseValue _Value;

			public MinuteParser(string minutePart) {

				if (string.IsNullOrEmpty(minutePart))
					throw new ArgumentNullException(nameof(minutePart));

				var rangeRegex = Regex.Match(minutePart, @"^(?<Min>\d+)-(?<Max>\d+)$");
				var stepsRegex = Regex.Match(minutePart, @"^(?<Start>\d+)/(?<Step>\d+)$");
				if (minutePart == "*")
					this._Value = new AnyValue();
				else if (rangeRegex.Success) {
					var min = int.Parse(rangeRegex.Result("${Min}"));
					var max = int.Parse(rangeRegex.Result("${Max}"));
					this._ValidateValue(min);
					this._ValidateValue(max);
					this._Value = new RangeValue(min, max);
				} else if (stepsRegex.Success) {
					var start = int.Parse(rangeRegex.Result("${Start}"));
					var step = int.Parse(rangeRegex.Result("${Step}"));
					this._ValidateValue(start);
					this._ValidateValue(step);
					this._Value = new StepValue(this._ExpandSteps(start, step));
				} else if (int.TryParse(minutePart, out var specificValue)) {
					this._ValidateValue(specificValue);
					this._Value = new SpecificValue(minutePart);
				} else
					throw new FormatException($"'{minutePart}' is invalid");
			}

			public static MinuteParser[] Parse(string minuteParts) {

				var match = Regex.Match(minuteParts, @"(?<value>[-\*0-9\/]+)(?:,(?<value>[-\*0-9\/]+))*");
				if (!match.Success)
					throw new FormatException($"'{minuteParts}' is invalid");

				var returnValue = new List<MinuteParser>();
				foreach (var item in minuteParts.Split(','))
					returnValue.Add(new MinuteParser(item));
				return returnValue.ToArray();
			}

			#region Helper Method(s)

			object[] _ExpandSteps(int start, int step) {

				var returnValue = new List<object>();
				returnValue.Add(start);
				var next = start;
				do {
					returnValue.Add(next);
					next += step;
				} while (next < 59);
				return returnValue.ToArray();
			}

			void _ValidateValue(int value) {
				if (value < 0 || value > 59)
					throw new FormatException($"Value '{value}' is out of range");
			}

			#endregion
		}

		sealed class HoursParser {

			readonly string _MinutePart;

			public HoursParser(string minutePart) {

				if (string.IsNullOrEmpty(minutePart))
					throw new ArgumentNullException(nameof(minutePart));
				if (!Validate(minutePart))
					throw new ArgumentException("Value is not valid", nameof(minutePart));
				this._MinutePart = minutePart;
				this.IsValid = false;
			}

			public bool IsValid { get; }

			public static bool Validate(string expression)
				=> false;
		}

		sealed class DaysParser {

			readonly string _MinutePart;

			public DaysParser(string minutePart) {

				if (string.IsNullOrEmpty(minutePart))
					throw new ArgumentNullException(nameof(minutePart));
				if (!Validate(minutePart))
					throw new ArgumentException("Value is not valid", nameof(minutePart));
				this._MinutePart = minutePart;
				this.IsValid = false;
			}

			public bool IsValid { get; }

			public static bool Validate(string expression)
				=> false;
		}

		sealed class MonthParser {

			readonly string _MinutePart;

			public MonthParser(string minutePart) {

				if (string.IsNullOrEmpty(minutePart))
					throw new ArgumentNullException(nameof(minutePart));
				if (!Validate(minutePart))
					throw new ArgumentException("Value is not valid", nameof(minutePart));
				this._MinutePart = minutePart;
				this.IsValid = false;
			}

			public bool IsValid { get; }

			public static bool Validate(string expression)
				=> false;
		}

		sealed class DayOfWeekParser {

			readonly string _MinutePart;

			public DayOfWeekParser(string minutePart) {

				if (string.IsNullOrEmpty(minutePart))
					throw new ArgumentNullException(nameof(minutePart));
				if (!Validate(minutePart))
					throw new ArgumentException("Value is not valid", nameof(minutePart));
				this._MinutePart = minutePart;
				this.IsValid = false;
			}

			public bool IsValid { get; }

			public static bool Validate(string expression)
				=> false;
		}

		interface IBaseValue {

			object[] Values();
		}

		sealed class AnyValue : IBaseValue {

			public object[] Values()
				=> null;
		}

		sealed class RangeValue : IBaseValue {

			readonly object _Min;

			readonly object _Max;

			public RangeValue(object min, object max) {
				this._Min = min ?? throw new ArgumentNullException(nameof(min));
				this._Max = max ?? throw new ArgumentNullException(nameof(max));
			}

			public object[] Values()
				=> new object[] { this._Min, this._Max };
		}

		sealed class SpecificValue : IBaseValue {

			readonly object _Value;

			public SpecificValue(object value)
				=> this._Value = value;

			public object[] Values()
				=> new object[] { this._Value };
		}

		sealed class StepValue : IBaseValue {

			readonly object[] _AllSteps;

			public StepValue(params object[] allSteps)
				=> this._AllSteps = allSteps ?? throw new ArgumentNullException(nameof(allSteps));

			public object[] Values()
				=> this._AllSteps;
		}

		#endregion
	}
}
