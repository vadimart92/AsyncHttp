namespace AsyncHttp
{
	using System;

	public interface IRetryStrategy
	{

		TimeSpan GetRetryDelay(int retryNumber);
		int GetRetryCount();

	}

}
