namespace System {

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