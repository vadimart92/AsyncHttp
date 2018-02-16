namespace AsyncHttp
{
	using System;

	class SimpleRetryStrategy : IRetryStrategy
	{
		private readonly int _simpleDelay;

		public SimpleRetryStrategy(int simpleDelay) {
			_simpleDelay = simpleDelay;
		}

		public TimeSpan GetRetryDelay(int retryNumber) {
			return TimeSpan.FromMilliseconds((retryNumber+1) * _simpleDelay);
		}

		public int GetRetryCount() {
			return 10;
		}
	}
}