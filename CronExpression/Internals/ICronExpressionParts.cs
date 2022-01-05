namespace CronExpression.Internals {

	public interface ICronExpressionParts {

		string Minutes { get; }

		string Hours { get; }

		string Days { get; }

		string Months { get; }

		string DayOfWeek { get; }
	}
}
