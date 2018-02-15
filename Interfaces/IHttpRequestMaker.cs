namespace AsyncHttp
{
	using System;
	using System.Threading.Tasks;

	public interface IHttpRequestMaker
	{
		/// <summary>Executes the GET request to specified URL.</summary>
		/// <param name="url">The URL.</param>
		/// <param name="body">The request body.</param>
		/// <param name="retryStrategy">The retry strategy.</param>
		/// <returns>Response BODY</returns>
		Task<string> Execute(string url, string body, IRetryStrategy retryStrategy);
	}
}