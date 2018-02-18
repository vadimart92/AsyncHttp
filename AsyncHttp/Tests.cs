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

		private static string Url = "http://tscore-dev-13:5000/api/ping";

		private static readonly SimpleRetryStrategy _retryStrategy = new SimpleRetryStrategy(200);

		private static string _payload = new String('1', 1_000_000);

		[Test]
		public async Task Actors() {
			ActorBasedHttpRequestMaker.Init();
			//var threadsCreated = Process.GetCurrentProcess().Threads.Count;
			ThreadPool.SetMinThreads(100, 500);
			ThreadPool.SetMaxThreads(100, 500);
			Url = "http://tscore-dev-13:5000/api/LoadTest/GetData?retryCount=1&delay=1000&errorCode=407";
			//Url = "http://v-artemchuk:5000/api/LoadTest/GetData?retryCount=1&delay=1000&errorCode=407";
			await RunTests();
		}

		private async Task RunTests() {
			TestContext.WriteLine($"URL: {Url}");
			GC.Collect(2, GCCollectionMode.Forced, true, true);
			GC.Collect(2, GCCollectionMode.Forced, true, true);
			var tplResults = await ExecuteRequests<TplRequestMaker>();
			TestContext.WriteLine($"TPL Time: {tplResults:ss\\.fff}");
			GC.Collect(2, GCCollectionMode.Forced, true, true);
			GC.Collect(2, GCCollectionMode.Forced, true, true);
			var results = await ExecuteRequests<ActorBasedHttpRequestMaker>();
			TestContext.WriteLine($"Time: {results:ss\\.fff}");
		}

		public async Task<TimeSpan> ExecuteRequests<TRequestMaker>()
				where TRequestMaker: IHttpRequestMaker, new() {
			var results = new ConcurrentBag<(string, bool)>();
			Stopwatch sw = null;
			const int requestsCount = 50;
			try {
				var tasks = new List<Task>();
				sw = Stopwatch.StartNew();
				for (int i = 0; i < requestsCount; i++) {
					var requestBody = $"{Guid.NewGuid()}{_payload}";
					var requestMaker = new TRequestMaker();
					tasks.Add(requestMaker.Execute(Url, requestBody, _retryStrategy).ContinueWith(t => {
						bool success = t.Result != null && t.Result.Equals(requestBody, StringComparison.OrdinalIgnoreCase);
						results.Add((requestBody, success));
					}, TaskContinuationOptions.ExecuteSynchronously)) ;
				}
				await Task.WhenAll(tasks.ToArray());
				sw.Stop();
			} catch (OperationCanceledException) {}
			var errors = results.Where(r => !r.Item2).ToList();
			if (errors.Any() || results.Count != requestsCount) {
				throw new Exception();
			}
			return sw.Elapsed;
		}

	}
}
