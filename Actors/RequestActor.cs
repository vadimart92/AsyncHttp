using System;
using System.Net;
using Akka.Actor;
using AsyncHttp;
using Interfaces;

namespace Actors
{
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
		private struct RetryCall { }
		private struct GetResponse { }
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
			Receive<GetResponse>(msg => {
				_currentRequest.GetResponseAsync().ContinueWith(task => {
					_currentRequest = null;
					if (task.IsFaulted) {
						return new ExecutionError { Exception =task.Exception };
					}
					try {
						var response = task.Result.GetResponseStream().GetContent();
						return (object)new ExecutionSuccess { Result = response };
					}
					catch (WebException e) {
						return new ExecutionError { Exception = e };
					}
				}).PipeTo(Self);
			});
			Receive<RetryAfterDelay>(call => {
				_retryAttempt++;
				var delay = _requestData.RetryStrategy.GetRetryDelay(_retryAttempt);
				Context.System.Scheduler.ScheduleTellOnce(delay, Self, new RetryCall(), Self);
			});
			Receive<RetryCall>(call => {
				ExecuteRequest();
			});
			Receive<ExecutionSuccess>(result => {
				UnbecomeStacked();
				_sender.Tell(result.Result);
			});
			Receive<ExecutionError>(result => {
				if (_retryAttempt < _requestData.RetryCount) {
					Self.Tell(new RetryAfterDelay(), Self);
					return;
				}
				UnbecomeStacked();
				_sender.Tell(result.Exception);
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
				BecomeStacked(Working);
				ExecuteRequest();
			});
		}

		private void ExecuteRequest() {
			_currentRequest = _requestData.Uri.CreateRequest(_requestData.Body);
			Self.Tell(new GetResponse());
		}
	}
}