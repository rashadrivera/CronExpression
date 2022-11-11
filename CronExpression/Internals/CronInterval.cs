using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CronExpression.Internals {

	abstract class GenericStepValue : ICronValue {

		protected readonly int ABSOLUTE_MAX;

		readonly ComputationDelegates _Delegates;

		readonly bool _IsAnyValue;

		protected readonly int Start;

		protected readonly int Step;

		public GenericStepValue(
			int start,
			int step,
			int absoluteMax,
			bool isAnyValue,
			ComputationDelegates delegates) {

			if (start < 0)
				throw new ArgumentOutOfRangeException(nameof(start));
			if (step < 1)
				throw new ArgumentOutOfRangeException(nameof(step));
			if (absoluteMax < 7)
				throw new ArgumentOutOfRangeException(nameof(absoluteMax));

			this.Start = start;
			this.Step = step;
			this._IsAnyValue = isAnyValue;
			this.ABSOLUTE_MAX = absoluteMax;
			this._Delegates = delegates ?? throw new ArgumentOutOfRangeException(nameof(delegates));
		}

		DateTimeOffset ICronValue.Values(DateTimeOffset target) {

			DateTimeOffset returnValue;
			var targetValue = this._Delegates.IntervalValue(target);
			var reduced = this._Delegates.Reduce(target);

			DateTimeOffset @fixed;
			if (this._IsAnyValue)
				@fixed = reduced;
			else
				@fixed = this._Delegates.Fixed(target, this.Start);

			if (reduced < @fixed)
				returnValue = this._Delegates.Adjust(target, this.Start - targetValue);
			else if (reduced == @fixed)
				returnValue = target;
			else {

				var q = from i in this.GenerateAllSteps(target)
						where i >= target
						select i;

				//if (!q.Any())
				//	returnValue = this._Delegates.Adjust(target, (this.ABSOLUTE_MAX - targetValue) + this.Start);
				//else {
				//	var interval = q.First();
				//	returnValue = this._Delegates.Adjust(target, interval - targetValue);
				//}
				returnValue = q.First();
			}

			if (target < returnValue)
				returnValue = this._Delegates.Reduce(returnValue);

			return returnValue;
		}

		protected abstract IEnumerable<DateTimeOffset> GenerateAllSteps(DateTimeOffset target);
	}

	sealed class ComputationDelegates {

		public ComputationDelegates(
			Func<DateTimeOffset, int, DateTimeOffset> adjust,
			Func<DateTimeOffset, int> intervalValue,
			Func<DateTimeOffset, DateTimeOffset> reduce,
			Func<DateTimeOffset, int, DateTimeOffset> @fixed) {
			this.Adjust = adjust ?? throw new ArgumentNullException(nameof(adjust));
			this.IntervalValue = intervalValue ?? throw new ArgumentNullException(nameof(intervalValue));
			this.Reduce = reduce ?? throw new ArgumentNullException(nameof(reduce));
			this.Fixed = @fixed ?? throw new ArgumentNullException(nameof(@fixed));
		}

		public Func<DateTimeOffset, int, DateTimeOffset> Adjust { get; }

		public Func<DateTimeOffset, int> IntervalValue { get; }

		public Func<DateTimeOffset, DateTimeOffset> Reduce { get; }

		public Func<DateTimeOffset, int, DateTimeOffset> Fixed { get; }
	}

	abstract class CronInterval : ICronInterval {

		readonly int _ABSOLUTE_MAX;

		readonly bool _IsZeroBasedValue;

		protected readonly ICronValue[] Values;

		public abstract DateTimeOffset ApplyInterval(DateTimeOffset target);

		public virtual bool IsWithinInterval(DateTimeOffset target)
			=> throw new NotImplementedException();

		#region Protected Method(s)

		protected CronInterval(int absoluteMax, string rawPartExpression, bool isZeroBasedValue) {

			if (absoluteMax < 7)
				throw new ArgumentOutOfRangeException(nameof(absoluteMax));
			if (string.IsNullOrEmpty(rawPartExpression))
				throw new ArgumentNullException(nameof(rawPartExpression));

			this._ABSOLUTE_MAX = absoluteMax;
			this._IsZeroBasedValue = isZeroBasedValue;

			var values = new List<ICronValue>(16);
			foreach (var i in this.DecomposePartExpression(rawPartExpression))
				// WARNING: THE FOLLOWING STATEMENT DEPENDS ON MULTIPLE MEMBER
				// FIELD VALUES AND MUST BE PROCESSED LAST
				values.Add(this.ExtractValue(i));

			this.Values = values.ToArray();
		}

		protected abstract DateTimeOffset AdjustValue(DateTimeOffset target, int value);

		protected IEnumerable<string> DecomposePartExpression(string rawPartExpression) {

			if (!Regex.IsMatch(rawPartExpression, @"(?<value>[-\*0-9\/]+)(?:,(?<value>[-\*0-9\/]+))*"))
				yield return rawPartExpression;
			else {
				var q = from i in rawPartExpression.Split(',')
						select i;
				foreach (var i in q)
					yield return i;
			}
		}

		protected internal ICronValue ExtractValue(string part) {

			if (string.IsNullOrEmpty(part))
				throw new ArgumentNullException(nameof(part));

			CronExpressionHelper.ValidateRawPartExpression(part, this._ABSOLUTE_MAX, this._IsZeroBasedValue);

			ICronValue returnValue;

			var rangeRegex = Regex.Match(part, @"^(?<Min>\d+)-(?<Max>\d+)$");
			var stepsRegex = Regex.Match(part, @"^(?<Start>\d+|\*)/(?<Step>\d+)$");
			if (part == "*")
				returnValue = new NoOp();
			else if (rangeRegex.Success) {
				var min = int.Parse(rangeRegex.Result("${Min}"));
				var max = int.Parse(rangeRegex.Result("${Max}"));
				returnValue = this.RangeValueFactory(min, max);
			} else if (stepsRegex.Success) {
				string startAsString = stepsRegex.Result("${Start}");
				var isAnyValue = false;
				int start;
				if (startAsString == "*") {
					start = this._IsZeroBasedValue ? 0 : 1;
					isAnyValue = true;
				} else
					start = int.Parse(startAsString);
				var step = int.Parse(stepsRegex.Result("${Step}"));
				returnValue = this.StepFactory(start, step, isAnyValue);
			} else if (int.TryParse(part, out var specificValue)) {
				returnValue = this.SpecificIntervalFactory(specificValue);
			} else
				throw new InvalidOperationException();

			return returnValue;
		}

		protected abstract DateTimeOffset Fixed(DateTimeOffset target, int value);

		protected abstract int IntervalValue(DateTimeOffset target);

		protected virtual ICronValue SpecificIntervalFactory(int specificInterval)
			=> new GenericSpecificValue(
				specificInterval,
				this._ABSOLUTE_MAX,
				new ComputationDelegates(
					this.AdjustValue,
					this.IntervalValue,
					this.Reduce,
					this.Fixed
				)
			);

		protected abstract ICronValue StepFactory(int start, int step, bool isAnyValue);

		protected virtual ICronValue RangeValueFactory(int min, int max)
			=> new GenericRangeValue(
				min,
				max,
				this._ABSOLUTE_MAX,
				new ComputationDelegates(
					this.AdjustValue,
					this.IntervalValue,
					this.Reduce,
					this.Fixed
				)
			);

		protected abstract DateTimeOffset Reduce(DateTimeOffset target);

		protected abstract bool ValidateValue(int value);

		#endregion

		#region Member Class(es)

		sealed class NoOp : ICronValue {

			public NoOp() { }

			DateTimeOffset ICronValue.Values(DateTimeOffset target)
				=> target;
		}

		sealed class GenericRangeValue : ICronValue {

			readonly int ABSOLUTE_MAX;

			readonly ComputationDelegates _Delegates;

			readonly int _RangeMinValue;

			readonly int _RangeMaxValue;

			public GenericRangeValue(int rangeMin, int rangeMax, int absoluteMax, ComputationDelegates delegates) {

				if (absoluteMax < 7)
					throw new ArgumentOutOfRangeException(nameof(absoluteMax));
				if (rangeMin < 0 || rangeMin >= absoluteMax)
					throw new ArgumentOutOfRangeException(nameof(rangeMin));
				if (rangeMax < 0 || rangeMax >= absoluteMax)
					throw new ArgumentOutOfRangeException(nameof(rangeMax));
				if (rangeMin >= rangeMax)
					throw new ArgumentOutOfRangeException(nameof(rangeMin), $"Value cannot be greater than max value of {rangeMax}");
				this._RangeMinValue = rangeMin;
				this._RangeMaxValue = rangeMax;
				this.ABSOLUTE_MAX = absoluteMax;
				this._Delegates = delegates ?? throw new ArgumentOutOfRangeException(nameof(delegates));
			}

			DateTimeOffset ICronValue.Values(DateTimeOffset target) {

				DateTimeOffset returnValue;
				var targetValue = this._Delegates.IntervalValue(target);
				var reduced = this._Delegates.Reduce(target);
				if (reduced > this._Delegates.Fixed(target, this._RangeMaxValue))
					returnValue = this._Delegates.Adjust(target, (this.ABSOLUTE_MAX - targetValue) + this._RangeMinValue);
				else if (reduced < this._Delegates.Fixed(target, this._RangeMinValue))
					returnValue = this._Delegates.Adjust(target, this._RangeMinValue - targetValue);
				else
					returnValue = target; // We are within range
				if (target < returnValue)
					returnValue = this._Delegates.Reduce(returnValue);
				return returnValue;
			}
		}

		sealed class GenericSpecificValue : ICronValue {

			readonly int ABSOLUTE_MAX;

			readonly ComputationDelegates _Delegates;

			readonly int _Value;

			public GenericSpecificValue(int value, int absoluteMax, ComputationDelegates delegates) {
				if (absoluteMax < 7)
					throw new ArgumentOutOfRangeException(nameof(absoluteMax));
				this._Value = value;
				this.ABSOLUTE_MAX = absoluteMax;
				this._Delegates = delegates ?? throw new ArgumentOutOfRangeException(nameof(delegates));
			}

			DateTimeOffset ICronValue.Values(DateTimeOffset target) {

				var returnValue = target;

				var targetValue = this._Delegates.IntervalValue(target);
				if (this._Delegates.Reduce(target) > this._Delegates.Fixed(target, this._Value))
					returnValue = this._Delegates.Adjust(returnValue, (this.ABSOLUTE_MAX - targetValue) + this._Value);
				else
					returnValue = this._Delegates.Adjust(returnValue, this._Value - targetValue);

				if (target < returnValue)
					returnValue = this._Delegates.Reduce(returnValue);

				return returnValue;
			}
		}

		#endregion
	}
}
