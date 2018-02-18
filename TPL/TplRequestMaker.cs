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

		public async Task<string> Execute(string url, string body, IRetryStrategy retryStrategy) {
			_retryStrategy = retryStrategy;
			_retryCount = retryStrategy.GetRetryCount();
			var content = await GetResponse(url, body);
			return content;
		}

		private async Task<string> GetResponse(string url, string body) {
			_retryAttempt = 0;
			HttpWebResponse response = null;
			while (response == null) {
				try {
					var webRequest = url.CreateRequest(body);
					response = (HttpWebResponse)webRequest.GetResponse();
					var content = response.GetResponseStream().GetContent();
					response.Close();
					return content;
				} catch (WebException e) {
					_retryException = e.ToString();
					response = e.Response as HttpWebResponse;
					response?.Close();
					if (_retryAttempt < _retryCount) {
						var retryDelay = _retryStrategy.GetRetryDelay(_retryAttempt);
						await Task.Delay(retryDelay).ConfigureAwait(false);
						_retryAttempt++;
						response = null;
					} else {
						if (response == null) {
							throw;
						}
					}
				}
			}
			return null;
		}
	}
}
