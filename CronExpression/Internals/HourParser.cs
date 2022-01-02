using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CronExpression.Internals {

	sealed class HourParser {

		readonly ICronValue[] _Values;

		public HourParser(params string[] partCollection) {

			if (partCollection == null)
				throw new ArgumentNullException(nameof(partCollection));

			var values = new List<ICronValue>(16);
			foreach (var i in partCollection)
				values.Add(this._ExtractValue(i));

			this._Values = values.ToArray();
		}

		public static HourParser Parse(string parts) {

			var match = Regex.Match(parts, @"(?<value>[-\*0-9\/]+)(?:,(?<value>[-\*0-9\/]+))*");
			if (!match.Success)
				throw new FormatException($"'{parts}' is invalid");

			var q = from i in parts.Split(',')
					select i;

			return new HourParser(q.ToArray());
		}

		public DateTimeOffset Apply(DateTimeOffset target) {

			var results = new List<DateTimeOffset>(16);

			foreach (var i in this._Values)
				results.Add(i.Values(target));

			var returnValue = results.Min();
			return returnValue;
		}

		#region Helper Method(s)

		ICronValue _ExtractValue(string part) {

			if (string.IsNullOrEmpty(part))
				throw new ArgumentNullException(nameof(part));

			ICronValue returnValue;

			var rangeRegex = Regex.Match(part, @"^(?<Min>\d+)-(?<Max>\d+)$");
			var stepsRegex = Regex.Match(part, @"^(?<Start>\d+)/(?<Step>\d+)$");
			if (part == "*")
				returnValue = new NoOp();
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
			} else if (int.TryParse(part, out var specificValue)) {
				this._ValidateValue(specificValue);
				returnValue = new SpecificValue(specificValue);
			} else
				throw new FormatException($"'{part}' is invalid");

			return returnValue;
		}

		void _ValidateValue(int value) {
			if (value < 0 || value > 23)
				throw new FormatException($"Value '{value}' is out of range");
		}

		#endregion

		#region Member Class(es)

		sealed class RangeValue : ICronValue {

			readonly int _Min;

			readonly int _Max;

			public RangeValue(int min, int max) {

				if (min < 0 || min > 23)
					throw new ArgumentOutOfRangeException(nameof(min));
				if (max < 0 || max > 23)
					throw new ArgumentOutOfRangeException(nameof(max));
				if (min > max)
					throw new ArgumentOutOfRangeException(nameof(min), $"Value cannot be greater than max value of {max}");
				this._Min = min;
				this._Max = max;
			}

			public DateTimeOffset Values(DateTimeOffset target) {

				if (target.Hour > this._Max)
					return target.AddMinutes(-target.Minute) + TimeSpan.FromHours((24 - target.Hour) + this._Min);
				if (target.Hour < this._Min)
					return target.AddMinutes(-target.Minute) + TimeSpan.FromHours(this._Min - target.Hour);
				return target; // We are within range
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

				if (target.Hour >= this._Value)
					return target.AddMinutes(-target.Minute) + TimeSpan.FromHours((24 - target.Hour) + this._Value);
				else
					return target.AddMinutes(-target.Minute) + TimeSpan.FromHours(this._Value - target.Hour);
			}
		}

		sealed class NoOp : ICronValue {

			public NoOp() { }

			public DateTimeOffset Values(DateTimeOffset target)
				=> target;
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

				if (target.Hour < this._Start)
					return target
						.AddMinutes(-target.Minute)
						.AddHours(this._Start - target.Hour);

				var q = from i in this._GenerateAllSteps()
						where i > target.Hour
						select i;

				var interval = q.First();
				return target
					.AddMinutes(-target.Minute)
					.AddHours(-target.Hour)
					.AddHours(interval);
			}

			IEnumerable<int> _GenerateAllSteps() {
				for (var i = this._Start + this._Step; i < 24; i += this._Step)
					yield return i;
			}
		}

		#endregion
	}
}
