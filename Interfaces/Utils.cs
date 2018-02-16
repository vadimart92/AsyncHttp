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
			HttpWebRequest webRequest = WebRequest.CreateHttp(url);
			webRequest.Method = "POST";
			webRequest.ReadWriteTimeout = 50000;
			webRequest.AppendHeaders(new Dictionary<string, string> {
				["Content-Type"] = "application/json"
			}).AppendBody(body);
			webRequest.Timeout = 50000;
			return webRequest;
		}

	}
}
