﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CronExpression.Internals {

	sealed class HourParser : Parser {

		const int MAX_VALUE = 24;

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
			if (target < returnValue)
				returnValue = _Reduce(returnValue);
			return returnValue;
		}

		#region Helper Method(s)

		static int _Value(DateTimeOffset target)
			=> target.Hour;

		static DateTimeOffset _Adjust(DateTimeOffset target, int value)
			=> target.AddHours(value);

		static DateTimeOffset _Reduce(DateTimeOffset target)
			=> target
				.AddMilliseconds(-target.Millisecond)
				.AddSeconds(-target.Second)
				.AddMinutes(-target.Minute);

		static DateTimeOffset _Fixed(DateTimeOffset target, int fixedValue)
			=> _Reduce(target)
			.AddHours(-target.Hour)
			.AddHours(fixedValue);

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
			if (value < 0 || value >= MAX_VALUE)
				throw new FormatException($"Value '{value}' is out of range");
		}

		#endregion

		#region Member Class(es)

		sealed class RangeValue : ICronValue {

			readonly int _MinValue;

			readonly int _MaxValue;

			public RangeValue(int min, int max) {

				if (min < 0 || min >= MAX_VALUE)
					throw new ArgumentOutOfRangeException(nameof(min));
				if (max < 0 || max >= MAX_VALUE)
					throw new ArgumentOutOfRangeException(nameof(max));
				if (min >= max)
					throw new ArgumentOutOfRangeException(nameof(min), $"Value cannot be greater than max value of {max}");
				this._MinValue = min;
				this._MaxValue = max;
			}

			public DateTimeOffset Values(DateTimeOffset target) {

				DateTimeOffset returnValue;
				var targetValue = _Value(target);
				var reduced = _Reduce(target);
				if (reduced > _Fixed(target, this._MaxValue))
					returnValue = _Reduce(_Adjust(target, (MAX_VALUE - targetValue) + this._MinValue));
				else if (reduced < _Fixed(target, this._MinValue))
					returnValue = _Reduce(_Adjust(target, this._MinValue - targetValue));
				else
					returnValue = target; // We are within range
				return returnValue;
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

				var returnValue = target;

				var targetValue = _Value(target);
				if (_Reduce(target) > _Fixed(target, this._Value))
					returnValue = _Reduce(_Adjust(returnValue, (MAX_VALUE - targetValue) + this._Value));
				else
					returnValue = _Adjust(returnValue, this._Value - targetValue);

				return returnValue;
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

				DateTimeOffset returnValue;
				var targetValue = _Value(target);
				var @fixed = _Fixed(target, this._Start);
				var reduced = _Reduce(target);
				if (reduced < @fixed)
					returnValue = _Reduce(_Adjust(target, this._Start - targetValue));
				else if (reduced == @fixed)
					returnValue = target;
				else {

					var q = from i in this._GenerateAllSteps()
							where i >= targetValue
							select i;

					if (!q.Any())
						returnValue = _Reduce(_Adjust(target, (MAX_VALUE - targetValue) + this._Start));
					else {
						var interval = q.First();
						returnValue = _Reduce(_Adjust(target, interval - targetValue));
					}
				}

				return returnValue;
			}

			IEnumerable<int> _GenerateAllSteps() {
				for (var i = this._Start + this._Step; i < MAX_VALUE; i += this._Step)
					yield return i;
			}
		}

		#endregion
	}
}
