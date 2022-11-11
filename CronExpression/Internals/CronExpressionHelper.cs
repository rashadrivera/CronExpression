using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace CronExpression.Internals {

	static class CronExpressionHelper {

		internal static ICronExpressionParts ExtractParts(string expression) {

			CronExpressionHelper.ValidateExpression(expression);
			var match = CronExpressionHelper.GetExpressionRegex(expression);

			return new CronExpressionParts {
				Minutes = match.Result("${Minutes}"),
				Hours = match.Result("${Hours}"),
				Days = match.Result("${Days}"),
				Months = match.Result("${Month}"),
				DayOfWeek = match.Result("${DayOfWeek}")
			};
		}

		internal static Match GetExpressionRegex(string expression)
			=> Regex.Match(
				expression,
				"(?<Minutes>[0-9*,/-]+) (?<Hours>[0-9*,/-]+) (?<Days>[0-9*,/-]+) (?<Month>[0-9*,/\\-a-z]+) (?<DayOfWeek>[0-9*,/\\-a-z]+)",
				RegexOptions.Singleline,
				TimeSpan.FromSeconds(0.2)
			);

		internal static void ValidateExpression(string expression) {

			Debug.Assert(!string.IsNullOrWhiteSpace(nameof(expression)), "Invalid method call");

			if (string.IsNullOrEmpty(expression?.Trim()))
				throw new ArgumentNullException(nameof(expression));

			var match = GetExpressionRegex(expression);
			if (!match.Success)
				throw new ArgumentException("Value is not a valid CRON expression", nameof(expression));

			_ValidateIntervalParts(match.Result("${Minutes}"), MinuteCronInterval.MAX_VALUE, true);
			_ValidateIntervalParts(match.Result("${Hours}"), HourCronInterval.MAX_VALUE, true);
			_ValidateIntervalParts(match.Result("${Days}"), DayCronInterval.MAX_VALUE, false);
			_ValidateIntervalParts(match.Result("${Month}"), MonthCronInterval.MAX_VALUE, false);
			_ValidateIntervalParts(match.Result("${DayOfWeek}"), DayOfWeekCronInterval.MAX_VALUE, true);
		}

		internal static void ValidateRawPartExpression(string rawPartExpression, int absoluteMax, bool isZeroBasedValue)
			=> _ValidateIntervalPart(rawPartExpression, absoluteMax, isZeroBasedValue);

		#region Helper Method(s)

		static void _ValidateIntervalPart(string rawPartExpression, int absoluteMax, bool isZeroBasedValue) {

			if (string.IsNullOrEmpty(rawPartExpression))
				throw new ArgumentNullException(nameof(rawPartExpression));

			var rangeRegex = Regex.Match(rawPartExpression, @"^(?<Min>\d+)-(?<Max>\d+)$");
			var stepsRegex = Regex.Match(rawPartExpression, @"^(?<Start>\d+|\*)/(?<Step>\d+)$");
			if (rawPartExpression == "*")
				return;
			else if (rangeRegex.Success) {
				var min = int.Parse(rangeRegex.Result("${Min}"));
				var max = int.Parse(rangeRegex.Result("${Max}"));
				if (!_ValidateValue(min, absoluteMax, isZeroBasedValue))
					throw new FormatException($"'{min}' is out of range for minimum value");
				if (!_ValidateValue(max, absoluteMax, isZeroBasedValue))
					throw new FormatException($"'{max}' is out of range for maximum value");
			} else if (stepsRegex.Success) {
				string startAsString = stepsRegex.Result("${Start}");
				int start;
				if (startAsString == "*")
					start = isZeroBasedValue ? 0 : 1;
				else
					start = int.Parse(startAsString);
				var step = int.Parse(stepsRegex.Result("${Step}"));
				if (!_ValidateValue(start, absoluteMax, isZeroBasedValue))
					throw new FormatException($"'{start}' is out of range for start value");
				if (!_ValidateValue(step, absoluteMax, isZeroBasedValue))
					throw new FormatException($"'{step}' is out of range for step value");
			} else if (int.TryParse(rawPartExpression, out var specificValue)) {
				if (!_ValidateValue(specificValue, absoluteMax, isZeroBasedValue))
					throw new FormatException($"'{specificValue}' is out of range for specific interval value");
			} else
				throw new FormatException($"'{rawPartExpression}' is an invalid expression");
		}

		static void _ValidateIntervalParts(string parts, int absoluteMax, bool isZeroBasedValue) {

			var match = Regex.Match(parts, @"(?<value>[-\*0-9\/]+)(?:,(?<value>[-\*0-9\/]+))*");
			var q = from i in parts.Split(',')
					select i;

			foreach (var rawPartExpression in q)
				_ValidateIntervalPart(rawPartExpression, absoluteMax, isZeroBasedValue);
		}

		static bool _ValidateValue(int value, int absoluteMax, bool isZeroBasedValue) {
			if (isZeroBasedValue)
				return value >= 0 && value < absoluteMax;
			else
				return value > 0 && value <= absoluteMax;
		}

		#endregion

		#region Member Class(es)

		sealed class CronExpressionParts : ICronExpressionParts {

			public string Minutes { get; set; }

			public string Hours { get; set; }

			public string Days { get; set; }

			public string Months { get; set; }

			public string DayOfWeek { get; set; }
		}

		#endregion
	}
}
