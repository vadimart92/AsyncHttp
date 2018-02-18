using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
	public static class Utils
	{
		public static HttpWebRequest CreateRequest(this string url, string body)
		{
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Method = "POST";
			webRequest.ReadWriteTimeout = 50_000;
			webRequest.AppendHeaders(new Dictionary<string, string> {
				["Content-Type"] = "application/json"
			});
			webRequest.AppendBody(body);
			webRequest.Timeout = 50_000;
			return webRequest;
		}
		public static async Task<HttpWebRequest> CreateRequestAsync(this string url, string body)
		{
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Method = "POST";
			webRequest.ReadWriteTimeout = 50000;
			webRequest.AppendHeaders(new Dictionary<string, string> {
				["Content-Type"] = "application/json"
			});
			await webRequest.AppendBodyAsync(body);
			webRequest.Timeout = 50000;
			return webRequest;
		}

	}
}
