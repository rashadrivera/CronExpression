using System;

namespace CronExpression.Internals {

	public interface ICronInterval {

		DateTimeOffset ApplyInterval(DateTimeOffset target);

		bool IsWithinInterval(DateTimeOffset target);
	}
}
