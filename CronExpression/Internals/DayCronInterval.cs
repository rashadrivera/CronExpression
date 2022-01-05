using System;
using System.Collections.Generic;
using System.Linq;

namespace CronExpression.Internals {

	sealed class DayCronInterval : CronInterval {

		internal const int MAX_VALUE = 32;

		internal DayCronInterval(string rawPartExpression)
			: base(MAX_VALUE, rawPartExpression, false) { }

		public override DateTimeOffset ApplyInterval(DateTimeOffset target) {

			var results = new List<DateTimeOffset>(16);

			foreach (var i in this.Values)
				results.Add(i.Values(target));

			var returnValue = results.Min();
			if (target < returnValue)
				returnValue = _Reduce(returnValue);
			return returnValue;
		}

		#region Helper Method(s)

		protected override int IntervalValue(DateTimeOffset target)
			=> _Value(target);

		protected override DateTimeOffset AdjustValue(DateTimeOffset target, int value)
			=> _AdjustValue(target, value);

		protected override DateTimeOffset Reduce(DateTimeOffset target)
			=> _Reduce(target);

		protected override DateTimeOffset Fixed(DateTimeOffset target, int fixedValue)
			=> _Fixed(target, fixedValue);

		static int _Value(DateTimeOffset target)
			=> target.Day;

		static DateTimeOffset _AdjustValue(DateTimeOffset target, int value)
			=> target.AddDays(value);

		static DateTimeOffset _Reduce(DateTimeOffset target)
			=> target
				.AddMilliseconds(-target.Millisecond)
				.AddSeconds(-target.Second)
				.AddMinutes(-target.Minute)
				.AddHours(-target.Hour);

		static DateTimeOffset _Fixed(DateTimeOffset target, int fixedValue)
			=> _Reduce(target)
			.AddDays(-target.Day)
			.AddDays(fixedValue);

		static int _Max(DateTimeOffset target) {
			switch (target.Month) {
				case 1:
					return 31;
				case 2:
					if (DateTime.IsLeapYear(target.Year))
						return 29;
					else
						return 28;
				case 3:
					return 31;
				case 4:
					return 30;
				case 5:
					return 30;
				case 6:
					return 30;
				case 7:
					return 31;
				case 8:
					return 31;
				case 9:
					return 30;
				case 10:
					return 31;
				case 11:
					return 30;
				case 12:
				default:
					return 31;
			}
		}

		protected override bool ValidateValue(int value)
			=> !(value <= 0 || value >= MAX_VALUE);

		protected override ICronValue RangeValueFactory(int min, int max)
			=> new RangeValue(min, max);

		protected override ICronValue StepFactory(int start, int step)
			=> new StepValue(start, step);

		protected override ICronValue SpecificIntervalFactory(int specificInterval)
			=> new SpecificValue(specificInterval);

		#endregion

		#region Member Class(es)

		sealed class RangeValue : ICronValue {

			readonly int _MinValue;

			readonly int _MaxValue;

			public RangeValue(int min, int max) {

				if (min <= 0 || min >= MAX_VALUE)
					throw new ArgumentOutOfRangeException(nameof(min));
				if (max <= 0 || max >= MAX_VALUE)
					throw new ArgumentOutOfRangeException(nameof(max));
				if (min >= max)
					throw new ArgumentOutOfRangeException(nameof(min), $"Value cannot be greater than max value of {max}");
				this._MinValue = min;
				this._MaxValue = max;
			}

			DateTimeOffset ICronValue.Values(DateTimeOffset target) {

				DateTimeOffset returnValue;
				var reduced = _Reduce(target);
				if (reduced > _Fixed(target, this._MaxValue))
					returnValue = _Reduce(_AdjustValue(target, (_Max(target) - _Value(target)) + this._MinValue));
				else if (reduced < _Fixed(target, this._MinValue)) {

					var seek = reduced;
					while (_Max(seek) < this._MinValue)
						seek = seek.AddMonths(1);
					returnValue = _Reduce(_AdjustValue(seek, this._MinValue - _Value(seek)));
				} else
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

			DateTimeOffset ICronValue.Values(DateTimeOffset target) {

				var returnValue = target;

				var targetValue = _Value(target);
				var reduced = _Reduce(target);
				if (reduced > _Fixed(target, this._Value)) {

					var seek = reduced
						.AddDays(_Max(reduced) - reduced.Day + 1);
					while (_Max(seek) < this._Value)
						seek = seek.AddMonths(1);
					returnValue = _Reduce(_AdjustValue(seek, this._Value - _Value(seek)));
				} else
					returnValue = _AdjustValue(returnValue, this._Value - targetValue);

				return returnValue;
			}
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

			DateTimeOffset ICronValue.Values(DateTimeOffset target) {

				DateTimeOffset returnValue;
				var targetValue = _Value(target);
				var @fixed = _Fixed(target, this._Start);
				var reduced = _Reduce(target);
				if (reduced < @fixed)
					returnValue = _Reduce(_AdjustValue(target, this._Start - targetValue));
				else if (reduced == @fixed)
					returnValue = target;
				else {

					var q = from i in this._GenerateAllSteps()
							where i >= targetValue
							select i;

					if (!q.Any())
						returnValue = _Reduce(_AdjustValue(target, (_Max(target) - targetValue) + this._Start));
					else {
						var interval = q.First();
						returnValue = _Reduce(_AdjustValue(target, interval - targetValue));
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
