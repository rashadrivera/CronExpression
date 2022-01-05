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
				.AddMinutes(-target.Hour)
				.AddDays(-(target.Day - 1));

		protected override DateTimeOffset Fixed(DateTimeOffset target, int fixedValue)
			=> this.Reduce(target)
				.AddMonths(-target.Month)
				.AddMonths(fixedValue);

		protected override bool ValidateValue(int value)
			=> !(value < 0 || value >= MAX_VALUE);

		#endregion
	}
}
