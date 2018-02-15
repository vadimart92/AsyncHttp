namespace AsyncHttp
{
	using System;

	class SimpleRetryStrategy : IRetryStrategy
	{

		public TimeSpan GetRetryDelay(int retryNumber) {
			return TimeSpan.FromMilliseconds(retryNumber * 100);
		}

	}
}