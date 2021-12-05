using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace System {

	// https://crontab.guru/

	public sealed class CronExpression {

		private readonly MinuteParser _Minutes;

		//private readonly HoursParser[] _Hours;

		//private readonly DaysParser[] _Days;

		//private readonly MonthParser[] _Month;

		//private readonly DayOfWeekParser[] _DayOfWeek;

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

		public DateTimeOffset Next(DateTimeOffset target) {

			var returnValue = this._Minutes.Apply(target);
			return returnValue;
		}

		public DateTimeOffset NextInterval(DateTimeOffset target, int offset)
			=> throw new NotImplementedException();

		public TimeSpan Offset() {
#error Left Off here
		}

		//public TimeSpan OffsetInterval(int offset)
		//	=> throw new NotImplementedException();

		#region Member Class(es)

		sealed class MinuteParser {

			readonly ICronValue[] _Values;

			public MinuteParser(params string[] minutePartCollection) {
				if (minutePartCollection == null)
					throw new ArgumentNullException(nameof(minutePartCollection));

				var values = new List<ICronValue>(16);
				foreach (var i in minutePartCollection)
					values.Add(this._ExtractValue(i));

				this._Values = values.ToArray();
			}

			public static MinuteParser Parse(string minuteParts) {

				var match = Regex.Match(minuteParts, @"(?<value>[-\*0-9\/]+)(?:,(?<value>[-\*0-9\/]+))*");
				if (!match.Success)
					throw new FormatException($"'{minuteParts}' is invalid");

				var q = from i in minuteParts.Split(',')
						select i;

				return new MinuteParser(q.ToArray());
			}

			public DateTimeOffset Apply(DateTimeOffset target) {

				var results = new List<TimeSpan>(16);

				foreach (var i in this._Values) {
					foreach (var v in i.Values().Cast<int>()) {
						results.Add(TimeSpan.FromMinutes(v));
					}
				}

				var closestInterval = results.Min();
				var returnValue = target + closestInterval;

				return returnValue;
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

			ICronValue _ExtractValue(string minutePart) {

				if (string.IsNullOrEmpty(minutePart))
					throw new ArgumentNullException(nameof(minutePart));

				ICronValue returnValue;

				var rangeRegex = Regex.Match(minutePart, @"^(?<Min>\d+)-(?<Max>\d+)$");
				var stepsRegex = Regex.Match(minutePart, @"^(?<Start>\d+)/(?<Step>\d+)$");
				if (minutePart == "*")
					returnValue = new AnyValue();
				else if (rangeRegex.Success) {
					var min = int.Parse(rangeRegex.Result("${Min}"));
					var max = int.Parse(rangeRegex.Result("${Max}"));
					this._ValidateValue(min);
					this._ValidateValue(max);
					returnValue = new RangeValue(min, max);
				} else if (stepsRegex.Success) {
					var start = int.Parse(stepsRegex.Result("${Start}"));
					var step = int.Parse(stepsRegex.Result("${Step}"));
					this._ValidateValue(start);
					this._ValidateValue(step);
					returnValue = new StepValue(this._ExpandSteps(start, step));
				} else if (int.TryParse(minutePart, out var specificValue)) {
					this._ValidateValue(specificValue);
					returnValue = new SpecificValue(specificValue);
				} else
					throw new FormatException($"'{minutePart}' is invalid");

				return returnValue;
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

		interface IRelativeCronValue {

			bool IsRelative { get; }

			TimeSpan Offset();
		}

		interface ICronValue { object[] Values(); }

		sealed class AnyValue : ICronValue {

			public object[] Values()
				=> null;
		}

		sealed class RangeValue : ICronValue {

			readonly object _Min;

			readonly object _Max;

			public RangeValue(object min, object max) {
				this._Min = min ?? throw new ArgumentNullException(nameof(min));
				this._Max = max ?? throw new ArgumentNullException(nameof(max));
			}

			public object[] Values()
				=> new object[] { this._Min, this._Max };
		}

		sealed class SpecificValue : ICronValue, IRelativeCronValue {

			readonly object _Value;

			public SpecificValue(object value)
				: this(value, false) { }

			public SpecificValue(object value, bool isRelative) {
				this._Value = value;
				this.IsRelative = isRelative;
			}

			public bool IsRelative { get; }

			public TimeSpan Offset() {

				if (!this.IsRelative)
					throw new InvalidOperationException();

#error Left Off here
				throw new NotImplementedException();
			}

			public object[] Values()
				=> new object[] { this._Value };
		}

		sealed class StepValue : ICronValue {

			readonly object[] _AllSteps;

			public StepValue(params object[] allSteps)
				=> this._AllSteps = allSteps ?? throw new ArgumentNullException(nameof(allSteps));

			public object[] Values()
				=> this._AllSteps;
		}

		#endregion
	}
}
