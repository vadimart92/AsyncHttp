namespace Interfaces
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Threading.Tasks;

	#region Class: HttpWebExtensions

	/// <summary>
	/// Contains extension methods for HttpWebRequest/HttpWebResponse, used by <see cref="ServiceClient"/>.
	/// </summary>
	public static class HttpWebExtensions
	{

		#region Constants: Private

		private const string CookieHeaderName = "Set-Cookie";

		#endregion

		#region Methods: Private

		private static IEnumerable<Cookie> FetchCookiesFromHeader(this HttpWebResponse source, Uri host) {
			var cookieHeaders = source.Headers.AllKeys.Where(key => key.StartsWith(CookieHeaderName));
			foreach (string cookieHeader in cookieHeaders) {
				var cookieContainer = new CookieContainer();
				cookieContainer.SetCookies(host, source.Headers[cookieHeader]);
				foreach (Cookie cookie in cookieContainer.GetCookies(host)) {
					yield return cookie;
				}
			}
		}

		#endregion

		#region Methods: public

		public static bool IsNullOrEmpty(this string source) {
			return string.IsNullOrEmpty(source);
		}
		public static bool IsNotNullOrEmpty(this string source) {
			return !string.IsNullOrEmpty(source);
		}
		public static string GetContent(this Stream source) {
			if (source == null) {
				throw new Exception("source");
			}
			using (source) {
				using (var sr = new StreamReader(source)) {
					return sr.ReadToEnd();
				}
			}
		}
		public static async Task<string> GetContentAsync(this Stream source) {
			if (source == null) {
				throw new Exception("source");
			}
			using (source) {
				using (var sr = new StreamReader(source)) {
					return await sr.ReadToEndAsync().ConfigureAwait(false);
				}
			}
		}
		public static async Task AppendBodyAsync(this WebRequest source, string body) {
			var bodyBytes = new UTF8Encoding(false).GetBytes(body);
			source.ContentLength = bodyBytes.Length;
			var requestStream = await source.GetRequestStreamAsync().ConfigureAwait(false);
			using (var sw = new StreamWriter(requestStream)) {
				await sw.WriteAsync(body).ConfigureAwait(false);
				await sw.FlushAsync().ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Writes body to HttpWebRequest request stream.
		/// </summary>
		/// <param name="source">Instance of HttpWebRequest.</param>
		/// <param name="body">Body content string.</param>
		/// <returns>Modified instance of HttpWebRequest.</returns>
		public static WebRequest AppendBody(this WebRequest source, string body) {
			if (body.IsNullOrEmpty()) {
				return source;
			}
			var bodyBytes = new UTF8Encoding(false).GetBytes(body);
			source.ContentLength = bodyBytes.Length;
			using (Stream requestStream = source.GetRequestStream()) {
				requestStream.Write(bodyBytes, 0, bodyBytes.Length);
				requestStream.Flush();
			}
			return source;
		}


		/// <summary>
		/// Appends header parameters to request.
		/// </summary>
		/// <param name="source">Instance of HttpWebRequest.</param>
		/// <param name="headerParameters">Collection of header names and values.</param>
		/// <returns>Modified instance of HttpWebRequest.</returns>
		public static HttpWebRequest AppendHeaders(this HttpWebRequest source,
			Dictionary<string, string> headerParameters) {
			foreach (var header in headerParameters) {
				if (header.Value.IsNullOrEmpty()) {
					continue;
				}

				switch (header.Key) {
					case "Accept":
						source.Accept = header.Value;
						break;
					case "Content-Type":
						source.ContentType = header.Value;
						break;
					case "Date":
						if (DateTime.TryParse(header.Value, out DateTime parsed)) {
							source.Date = parsed;
						}

						break;
					case "Host":
						source.Host = header.Value;
						break;
					default:
						source.Headers.Add(header.Key, header.Value);
						break;
				}
			}

			return source;
		}

		/// <summary>
		/// Appends cookie parameters to request.
		/// </summary>
		/// <param name="source">Instance of HttpWebRequest.</param>
		/// <param name="cookieParameters">Collection of cookie names and values.</param>
		/// <returns>Modified instance of HttpWebRequest.</returns>
		public static HttpWebRequest AppendCookies(this HttpWebRequest source,
			Dictionary<string, string> cookieParameters) {
			if (!cookieParameters.Any()) {
				return source;
			}

			if (source.CookieContainer == null) {
				source.CookieContainer = new CookieContainer(cookieParameters.Count);
			}

			foreach (var cookie in cookieParameters) {
				if (cookie.Value.IsNullOrEmpty()) {
					continue;
				}

				var requestCookie = new Cookie {
					Name = cookie.Key,
					Value = cookie.Value,
					Domain = source.RequestUri.Host
				};
				source.CookieContainer.Add(requestCookie);
			}

			return source;
		}

		/// <summary>
		/// Reads body content from HttpWebResponse.
		/// </summary>
		/// <param name="source">Incoming HttpWebResponse instance.</param>
		/// <returns>Body content string.</returns>
		public static string ReadoutBody(this HttpWebResponse source) {
			Stream responseStream = source.GetResponseStream();
			return responseStream != null ? responseStream.GetContent() : string.Empty;
		}

		/// <summary>
		/// Reads header parameters into Dictionary.
		/// </summary>
		/// <param name="source">Incoming HttpWebResponse instance.</param>
		/// <returns>Dictionary with found header parameters.</returns>
		public static Dictionary<string, string> ReadoutHeaders(this HttpWebResponse source) {
			var headerDictionary = new Dictionary<string, string>();
			foreach (string headerKey in source.Headers.Keys) {
				string headerValue = source.Headers[headerKey];
				if (!headerValue.IsNullOrEmpty()) {
					headerDictionary.Add(headerKey, headerValue);
				}
			}

			return headerDictionary;
		}

		/// <summary>
		/// Reads cookie parameters into Dictionary.
		/// </summary>
		/// <param name="source">Incoming HttpWebResponse instance.</param>
		/// <param name="host">Host to fetch cookies for.</param>
		/// <returns>Dictionary with found header parameters.</returns>
		public static Dictionary<string, string> ReadoutCookies(this HttpWebResponse source, Uri host) {
			var cookieDictionary = new Dictionary<string, string>();
			if (source.Cookies.Count != 0) {
				foreach (Cookie cookie in source.Cookies) {
					if (!cookie.Value.IsNullOrEmpty()) {
						cookieDictionary.Add(cookie.Name, cookie.Value);
					}
				}
			} else if (source.Headers[CookieHeaderName].IsNotNullOrEmpty()) {
				foreach (Cookie cookie in source.FetchCookiesFromHeader(host)) {
					cookieDictionary.Add(cookie.Name, cookie.Value);
				}
			}

			return cookieDictionary;
		}

		/// <summary>
		/// Returns true if http status code lays in arrange 200-399
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static bool IsSuccess(this HttpStatusCode source) {
			var code = (int)source;
			return code >= 200 && code < 400;
		}

		#endregion

	}

	#endregion

}
