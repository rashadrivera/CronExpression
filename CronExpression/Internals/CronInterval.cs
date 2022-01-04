using System;
using System.Text.RegularExpressions;

namespace CronExpression.Internals {

	abstract class CronInterval : ICronInterval {

		public abstract DateTimeOffset ApplyInterval(DateTimeOffset target);

		public virtual bool IsWithinInterval(DateTimeOffset target)
			=> throw new NotImplementedException();

		protected abstract bool ValidateValue(int value);

		protected abstract ICronValue RangeValueFactory(int min, int max);

		protected abstract ICronValue StepFactory(int start, int step);

		protected abstract ICronValue SpecificIntervalFactory(int specificInterval);

		protected internal ICronValue _ExtractValue(string part) {

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
				if (!this.ValidateValue(min))
					throw new FormatException($"'{min}' is out of range for minimum value");
				if (!this.ValidateValue(max))
					throw new FormatException($"'{max}' is out of range for maximum value");
				returnValue = this.RangeValueFactory(min, max);
			} else if (stepsRegex.Success) {
				var start = int.Parse(stepsRegex.Result("${Start}"));
				var step = int.Parse(stepsRegex.Result("${Step}"));
				if (!this.ValidateValue(start))
					throw new FormatException($"'{start}' is out of range for start value");
				if (!this.ValidateValue(step))
					throw new FormatException($"'{step}' is out of range for step value");
				returnValue = this.StepFactory(start, step);
			} else if (int.TryParse(part, out var specificValue)) {
				if (!this.ValidateValue(specificValue))
					throw new FormatException($"'{specificValue}' is out of range for specific interval value");
				returnValue = this.SpecificIntervalFactory(specificValue);
			} else
				throw new FormatException($"'{part}' is an invalid expression");

			return returnValue;
		}

		sealed class NoOp : ICronValue {

			public NoOp() { }

			public DateTimeOffset Values(DateTimeOffset target)
				=> target;
		}
	}
}
