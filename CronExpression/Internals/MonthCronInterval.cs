using System;
using System.Collections.Generic;
using System.Linq;

namespace CronExpression.Internals {

	sealed class MonthCronInterval : CronInterval {

		internal const int MAX_VALUE = 12;

		internal MonthCronInterval(string rawPartExpression)
			: base(MAX_VALUE, rawPartExpression, false) { }

		public override DateTimeOffset ApplyInterval(DateTimeOffset target) {

			var results = new List<DateTimeOffset>(16);

			foreach (var i in this.Values)
				results.Add(i.Values(target));

			var returnValue = results.Min();
			if (target < returnValue)
				returnValue = this.Reduce(returnValue);
			return returnValue;
		}

		#region Helper Method(s)

		protected override int IntervalValue(DateTimeOffset target)
			=> target.Month;

		protected override DateTimeOffset AdjustValue(DateTimeOffset target, int value)
			=> target.AddMonths(value);

		protected override DateTimeOffset Reduce(DateTimeOffset target)
			=> target
				.AddMilliseconds(-target.Millisecond)
				.AddSeconds(-target.Second)
				.AddMinutes(-target.Minute)
				.AddHours(-target.Hour)
				.AddDays(-(target.Day - 1));

		protected override DateTimeOffset Fixed(DateTimeOffset target, int fixedValue)
			=> this.Reduce(target)
				.AddMonths(-target.Month)
				.AddMonths(fixedValue);

		protected override bool ValidateValue(int value)
			=> !(value < 0 || value >= MAX_VALUE);

		protected override ICronValue StepFactory(int start, int step, bool isAnyValue)
			=> new MonthStepValue(
				start,
				step,
				MAX_VALUE,
				isAnyValue,
				new ComputationDelegates(
					this.AdjustValue,
					this.IntervalValue,
					this.Reduce,
					this.Fixed
				)
			);

		#endregion

		#region Member Class(es)

		sealed class MonthStepValue : GenericStepValue {

			public MonthStepValue(
				int start,
				int step,
				int absoluteMax,
				bool isAnyValue,
				ComputationDelegates delegates)
				: base(start, step, absoluteMax, isAnyValue, delegates) { }

			protected override IEnumerable<DateTimeOffset> GenerateAllSteps(DateTimeOffset target) {
				var MAX_LIMIT = target.AddDays(365 * 2);
				var returnValue = target.AddMonths(-target.Month)
					.AddMonths(this.Start);
				do {
					yield return returnValue;
					returnValue = returnValue.AddMonths(this.Step);
				} while (returnValue < MAX_LIMIT);
			}
		}

		#endregion
	}
}
