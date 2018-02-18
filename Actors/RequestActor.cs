using System;
using System.Net;
using Akka.Actor;
using AsyncHttp;
using Interfaces;

namespace Actors
{
	using System.Diagnostics;
	using System.IO;
	using System.Threading.Tasks;

	public class RequestActor:ReceiveActor
	{
		internal class ExecuteRequestMsg
		{
			public string Uri { get; set; }
			public string Body { get; set; }
			public int RetryCount { get; set; }
			public IRetryStrategy RetryStrategy { get; set; }
			public string RequestId { get; set; }
		}
		private class RetryCall { }
		private class GetResponse { }
		private class RetryAfterDelay{}
		private class ExecutionError
		{
			public Exception Exception { get; set; }
		}
		private class ExecutionSuccess
		{
			public string Result { get; set; }
		}

		private IActorRef _sender;
		private ExecuteRequestMsg _requestData;
		private int _retryAttempt;
		private HttpWebRequest _currentRequest;

		public RequestActor() {
			Ready();
		}

		private void Working() {
			Receive<HttpWebRequest>(msg => {
				Debug.WriteLine($"Receive WebRequest");
				msg.GetResponseAsync().PipeTo(Self);
			});
			Receive<WebResponse>(webResponse => {
				Debug.WriteLine("Receive WebResponse");
				webResponse.GetResponseStream().GetContentAsync().ContinueWith(task => {
					if (task.IsFaulted) {
						return (object)new ExecutionError { Exception = task.Exception };
					}
					var response = task.Result;
					webResponse.Close();
					return new ExecutionSuccess { Result = response };
				}, TaskContinuationOptions.ExecuteSynchronously).PipeTo(Self);
				
			});
			Receive<RetryAfterDelay>(call => {
				Debug.WriteLine($"Receive RetryAfterDelay");
				_retryAttempt++;
				var delay = _requestData.RetryStrategy.GetRetryDelay(_retryAttempt);
				Context.System.Scheduler.ScheduleTellOnce(delay, Self, new RetryCall(), Self);
			});
			Receive<RetryCall>(call => {
				Debug.WriteLine("Receive RetryCall");
				ExecuteRequest();
			});
			Receive<ExecutionSuccess>(result => {
				Debug.WriteLine("Receive ExecutionSuccess");
				_sender.Tell(result.Result);
				Context.Stop(Self);
			});
			Receive<Status.Failure>(result => {
				Debug.WriteLine("Receive ExecutionError");
				if (_retryAttempt < _requestData.RetryCount) {
					Self.Tell(new RetryAfterDelay(), Self);
					return;
				}
				_sender.Tell(result.Cause.ToString());
				Context.Stop(Self);
			});
		}

		protected override void PostStop() {
			base.PostStop();
			_currentRequest?.Abort();
		}

		private void Ready() {
			_retryAttempt = 0;
			Receive<ExecuteRequestMsg>(m => {
				_sender = Sender;
				_requestData = m;
				Become(Working);
				ExecuteRequest();
			});
		}

		private void ExecuteRequest() {
			_requestData.Uri.CreateRequestAsync(_requestData.Body).PipeTo(Self);
		}
	}
}