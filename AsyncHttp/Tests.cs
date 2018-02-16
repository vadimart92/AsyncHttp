using System.Collections.Generic;
using TPL;

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

		private static readonly SimpleRetryStrategy _retryStrategy = new SimpleRetryStrategy(200);
		
		[Test]
		public void Actors() {
			ActorBasedHttpRequestMaker.Init();
			//var threadsCreated = Process.GetCurrentProcess().Threads.Count;
			ThreadPool.SetMinThreads(50, 50);
			var results = ExecuteRequests<ActorBasedHttpRequestMaker>();
			TestContext.WriteLine($"Time: {results:ss\\.fff}");
			var tplResults = ExecuteRequests<TplRequestMaker>();
			TestContext.WriteLine($"TPL Time: {tplResults:ss\\.fff}");
			//TestContext.WriteLine($"Threads used: {Process.GetCurrentProcess().Threads.Count- threadsCreated}");
		}

		public TimeSpan ExecuteRequests<TRequestMaker>()
		where TRequestMaker: IHttpRequestMaker, new() {
			var results = new ConcurrentBag<(string, string)>();
			Stopwatch sw = null;
			const int requestsCount = 1000;
			try {
				var tasks = new List<Task>();
				sw = Stopwatch.StartNew();
				for (int i = 0; i < requestsCount; i++) {
					tasks.Add(Task.Run(async () => {
						var requestBody = Guid.NewGuid().ToString();
						var requestMaker = new TRequestMaker();
						var result = await requestMaker.Execute("http://127.0.0.1:5000/api/LoadTest/GetData?retryCount=3&delay=800", requestBody, _retryStrategy).ConfigureAwait(false);
						results.Add((requestBody, result));
					})) ;
				}
				Task.WaitAll(tasks.ToArray());
				sw.Stop();
			} catch (OperationCanceledException) {}
			var errors = results.Any(r => !r.Item2.Equals(Convert.ToBase64String(Encoding.UTF8.GetBytes(r.Item1)), StringComparison.OrdinalIgnoreCase));
			if (errors || results.Count != requestsCount) {
				return TimeSpan.FromDays(1);
			}
			return sw.Elapsed;
		}

	}
}
