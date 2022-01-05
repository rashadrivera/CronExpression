namespace System {

	/// <summary>
	/// Thrown when a CRON expression potentially takes an inordinate
	/// amount of time to process.
	/// </summary>
	public sealed class RunawayCronExpressionException : Exception {

		public RunawayCronExpressionException(string expression)
			: base($"Expression '{_Validate(expression)}' could not be processed in a timely manner") { }

		static string _Validate(string expression) {
			if (string.IsNullOrEmpty(expression))
				throw new ArgumentNullException(nameof(expression));
			return expression;
		}
	}
}