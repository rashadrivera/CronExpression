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

			var targetMinusSecondsDown = target
				.AddSeconds(-target.Second)
				.AddMilliseconds(-target.Millisecond);

			var returnValue = this._Minutes
				.Apply(targetMinusSecondsDown);

			return returnValue;
		}

		#region Member Class(es)

		interface ICronValue {

			DateTimeOffset Values(DateTimeOffset target);
		}

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

				var results = new List<DateTimeOffset>(16);

				foreach (var i in this._Values)
					results.Add(i.Values(target));

				var returnValue = results.Min();
				return returnValue;
			}

			#region Helper Method(s)

			ICronValue _ExtractValue(string minutePart) {

				if (string.IsNullOrEmpty(minutePart))
					throw new ArgumentNullException(nameof(minutePart));

				ICronValue returnValue;

				var rangeRegex = Regex.Match(minutePart, @"^(?<Min>\d+)-(?<Max>\d+)$");
				var stepsRegex = Regex.Match(minutePart, @"^(?<Start>\d+)/(?<Step>\d+)$");
				if (minutePart == "*")
					returnValue = new NextMinuteValue();
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
					returnValue = new StepValue(start, step);
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

			#region Member Class(es)

			sealed class RangeValue : ICronValue {

				readonly int _Min;

				readonly int _Max;

				public RangeValue(int min, int max) {

					if (min < 0 || min > 59)
						throw new ArgumentOutOfRangeException(nameof(min));
					if (max < 0 || max > 59)
						throw new ArgumentOutOfRangeException(nameof(max));
					if (min > max)
						throw new ArgumentOutOfRangeException(nameof(min), $"Value cannot be greater than max value of {max}");
					this._Min = min;
					this._Max = max;
				}

				public DateTimeOffset Values(DateTimeOffset target) {

					if (target.Minute > this._Max)
						return target + TimeSpan.FromMinutes((60 - target.Minute) + this._Min);
					if (target.Minute < this._Min)
						return target + TimeSpan.FromMinutes(this._Min - target.Minute);
					return target.AddMinutes(1); // We are within range
				}
			}

			sealed class SpecificValue : ICronValue {

				readonly int _Value;

				public SpecificValue(int value)
					: this(value, false) { }

				public SpecificValue(int value, bool isRelative) {
					this._Value = value;
					this.IsRelative = isRelative;
				}

				public bool IsRelative { get; }

				public DateTimeOffset Values(DateTimeOffset target) {

					if (target.Minute >= this._Value)
						return target + TimeSpan.FromMinutes((60 - target.Minute) + this._Value);
					else
						return target + TimeSpan.FromMinutes(this._Value - target.Minute);
				}
			}

			sealed class NextMinuteValue : ICronValue {

				public NextMinuteValue() { }

				public DateTimeOffset Values(DateTimeOffset target)
					=> target.AddMinutes(1);
			}

			sealed class StepValue : ICronValue {

				readonly int _Start;

				readonly int _Step;

				public StepValue(int start, int step) {

					if (start < 0)
						throw new ArgumentOutOfRangeException(nameof(start));

					this._Start = start;
					this._Step = step;
				}

				public DateTimeOffset Values(DateTimeOffset target) {

					if (target.Minute < this._Start)
						return target.AddMinutes(this._Start - target.Minute);

					var q = from i in this._GenerateAllSteps()
							where i > target.Minute
							select i;

					var interval = q.First();
					return target
						.AddMinutes(-target.Minute)
						.AddMinutes(interval);
				}

				IEnumerable<int> _GenerateAllSteps() {
					for (var i = this._Start + this._Step; i < 60; i += this._Step)
						yield return i;
				}
			}

			#endregion
		}

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
