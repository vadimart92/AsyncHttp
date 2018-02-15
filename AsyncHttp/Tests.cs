namespace AsyncHttp
{
	using System;
	using System.Collections.Concurrent;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Actors;
	using NUnit.Framework;

	[TestFixture]
	public class Tests
	{

		private static readonly SimpleRetryStrategy _retryStrategy = new SimpleRetryStrategy();
		
		[Test]
		public void Actors() {
			var requestMaker = new ActorBasedHttpRequestMaker();
			var results = ExecuteRequests(requestMaker).TotalMilliseconds;
			var sleepRequestMaker = new SleepActorBasedHttpRequestMaker();
			var sleepResults = ExecuteRequests(sleepRequestMaker).TotalMilliseconds;
			var taskSleepRequestMaker = new TaskSleepActorBasedHttpRequestMaker();
			var taskSleepResults = ExecuteRequests(taskSleepRequestMaker).TotalMilliseconds;
		}

		public TimeSpan ExecuteRequests(IHttpRequestMaker requestMaker) {
			var results = new ConcurrentBag<(string, string)>();
			var cts = new CancellationTokenSource();
			var options = new ParallelOptions {
				CancellationToken = cts.Token,
				MaxDegreeOfParallelism = 20
			};
			cts.Token.ThrowIfCancellationRequested();
			Stopwatch sw = null;
			try {
				sw = Stopwatch.StartNew();
				Parallel.For(0, 1000, options, async number => {
					var requestBody = Guid.NewGuid().ToString();
					var result = await requestMaker.Execute("test.com", requestBody, _retryStrategy);
					results.Add((requestBody, result));
				});
				sw.Stop();
				
			} catch (OperationCanceledException) {}
			var validResults = results.Count(r => r.Item2.Equals(Convert.ToBase64String(Encoding.UTF8.GetBytes(r.Item1)), StringComparison.OrdinalIgnoreCase));
			return TimeSpan.FromMilliseconds(validResults>0?sw.ElapsedMilliseconds/validResults:sw.ElapsedMilliseconds);
		}

	}
}
