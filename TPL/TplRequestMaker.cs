using System.Threading.Tasks;
using Interfaces;

namespace TPL
{
	using System.Collections.Generic;
	using System.Net;
	using AsyncHttp;

	public class TplRequestMaker : IHttpRequestMaker
	{

		private int _retryAttempt;
		private int _retryCount;
		private string _retryException ;
		private IRetryStrategy _retryStrategy;

		public Task<string> Execute(string url, string body, IRetryStrategy retryStrategy) {
			_retryStrategy = retryStrategy;
			_retryCount = retryStrategy.GetRetryCount();
			return Task.Run(() => {
				HttpWebResponse webResponse = GetResponse(url, body);
				return webResponse.GetResponseStream().GetContent();
			});
		}

		

		private HttpWebResponse GetResponse(string url, string body) {
			_retryAttempt = 0;
			HttpWebResponse response = null;
			while (response == null) {
				try {
					var webRequest = url.CreateRequest(body);
					response = (HttpWebResponse)webRequest.GetResponse();
				} catch (WebException e) {
					_retryException = e.ToString();
					response = e.Response as HttpWebResponse;
					if (_retryAttempt < _retryCount) {
						var retryDelay = _retryStrategy.GetRetryDelay(_retryAttempt);
						var delay = Task.Delay(retryDelay);
						delay.ConfigureAwait(false);
						delay.Wait();
						_retryAttempt++;
						response = null;
					} else {
						if (response == null) {
							throw;
						}
					}
				}
			}
			return response;
		}
	}
}
