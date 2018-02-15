using System.Threading.Tasks;

namespace Actors
{
	using System;
	using System.Text;
	using System.Threading;
	using AsyncHttp;
    public class ActorBasedHttpRequestMaker : IHttpRequestMaker
    {

		public async Task<string> Execute(string url, string body, IRetryStrategy retryStrategy) {
			await Task.Delay(retryStrategy.GetRetryDelay(1));
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(body));
		}

	}
    public class SleepActorBasedHttpRequestMaker : IHttpRequestMaker
    {

		public Task<string> Execute(string url, string body, IRetryStrategy retryStrategy) {
			Thread.Sleep(retryStrategy.GetRetryDelay(1));
			return Task.FromResult(Convert.ToBase64String(Encoding.UTF8.GetBytes(body)));
		}

	}
    public class TaskSleepActorBasedHttpRequestMaker : IHttpRequestMaker
    {

		public Task<string> Execute(string url, string body, IRetryStrategy retryStrategy) {
			return Task.Run(() => {
				Thread.Sleep(retryStrategy.GetRetryDelay(1));
				return Convert.ToBase64String(Encoding.UTF8.GetBytes(body));
			});
		}

	}
}
