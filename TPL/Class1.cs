using System;
using System.Threading.Tasks;

namespace TPL
{
	using System.Collections.Generic;
	using System.Net;
	using AsyncHttp;
	using Interfaces;

	public class Class1 : IHttpRequestMaker
	{

		private int _retryAttempt = 0;
		private int _retryCount = 0;
		private string _retryException ;
		private IRetryStrategy _retryStrategy;

		public Task<string> Execute(string url, string body, IRetryStrategy retryStrategy) {
			_retryStrategy = retryStrategy;
			_retryCount = retryStrategy.GetRetryCount();
			return Task.Run(() => {
				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
				webRequest.Method = "GET";
				webRequest.AppendHeaders(new Dictionary<string, string> {
					["Content-Type"] = "application/json"
				}).AppendBody(body);
				HttpWebResponse webResponse = GetResponse(webRequest);
				return webResponse.GetResponseStream().GetContent();
			});
		}

		private HttpWebResponse GetResponse(WebRequest webRequest) {
			_retryAttempt = 0;
			HttpWebResponse response = null;
			while (response == null) {
				try {
					response = (HttpWebResponse)webRequest.GetResponse();
				} catch (WebException e) {
					_retryException = e.ToString();
					response = e.Response as HttpWebResponse;
					if (_retryAttempt < _retryCount) {
						Task.Delay(_retryStrategy.GetRetryDelay(_retryAttempt)).Wait();
						_retryAttempt++;
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
