using System;
using System.Collections.Generic;
using System.Linq;

namespace CronExpression.Internals {

	sealed class MinuteCronInterval : CronInterval {

		internal const int MAX_VALUE = 60;

		internal MinuteCronInterval(string rawPartExpression)
			: base(MAX_VALUE, rawPartExpression, true) { }

		public override DateTimeOffset ApplyInterval(DateTimeOffset target) {

			var results = new List<DateTimeOffset>(16);

			foreach (var i in this.Values)
				results.Add(i.Values(target));

			var returnValue = results.Min();
			// ONLY MINUTES REDUCE AT THE END TO REMOVE
			// SECONDS AND MILLISECONDS
			return this.Reduce(returnValue);
		}

		#region Helper Method(s)

		protected override int IntervalValue(DateTimeOffset target)
			=> target.Minute;

		protected override DateTimeOffset AdjustValue(DateTimeOffset target, int value)
			=> target.AddMinutes(value);

		protected override DateTimeOffset Reduce(DateTimeOffset target)
			=> target
				.AddMilliseconds(-target.Millisecond)
				.AddSeconds(-target.Second);

		protected override DateTimeOffset Fixed(DateTimeOffset target, int fixedValue)
			=> this.Reduce(target)
				.AddMinutes(-target.Minute)
				.AddMinutes(fixedValue);

		protected override bool ValidateValue(int value)
			=> !(value < 0 || value >= MAX_VALUE);

		#endregion
	}
}
