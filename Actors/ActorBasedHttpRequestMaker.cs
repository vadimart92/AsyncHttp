using System.Threading.Tasks;
using Akka.Actor;

namespace Actors
{
	using System;
	using AsyncHttp;
    public class ActorBasedHttpRequestMaker : IHttpRequestMaker
    {
		public async Task<string> Execute(string url, string body, IRetryStrategy retryStrategy) {
			return await _root.Ask<string>(new RequestActor.ExecuteRequestMsg {
				Uri = url,
				Body = body,
				RetryCount = retryStrategy.GetRetryCount(),
				RetryStrategy = retryStrategy,
				RequestId = Guid.NewGuid().ToString()
			}).ConfigureAwait(false);
		}

	    private static ActorSystem _actorSystem;
	    private static IActorRef _root;
	    public static void Init() {
			_actorSystem = ActorSystem.Create("test");
		    _root = _actorSystem.ActorOf(Props.Create<RequestRootActor>());
		}
    }

	public class RequestRootActor : ReceiveActor
	{
		public struct Cancel
		{
			public string RequestId { get; set; }
		}

		public RequestRootActor() {
			Receive<RequestActor.ExecuteRequestMsg>(msg => {
				var child = GetChild(msg.RequestId);
				child.Forward(msg);
			});
			Receive<Cancel>(cancel => {
				var child = GetChild(cancel.RequestId);
				Context.Stop(child);
			});
		}

		private static IActorRef GetChild(string requestId) {
			var child = Context.Child(requestId);
			if (child.IsNobody()) {
				child = Context.ActorOf(Props.Create<RequestActor>());
			}

			return child;
		}
	}
}
