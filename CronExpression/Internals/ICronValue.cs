using System;

namespace CronExpression.Internals {

	interface ICronValue {

		DateTimeOffset Values(DateTimeOffset target);
	}
}
