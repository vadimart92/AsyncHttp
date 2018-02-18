using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace WebService.Controllers
{
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography;

	[Route("api/[controller]")]
	public class LoadTestController : Controller
	{
		private readonly IMemoryCache _memoryCache;

		private static readonly SHA512 _sha256 = SHA512.Create();
		public LoadTestController(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		[HttpGet("Ping")]
		public string Ping()
		{
			return "pong";
		}

		// GET api/values
		[HttpPost("getData")]
		public async Task<IActionResult> GetData([FromBody]string body, int retryCount, int delay, int errorCode) {
			if (string.IsNullOrWhiteSpace(body) || body.Length < 36) {
				Console.WriteLine($"{DateTime.Now:mm:ss.ffff} empty or small body {body}");
				return BadRequest("empty or small body");
			}
			await Task.Delay(TimeSpan.FromMilliseconds(delay));
			var bodyBytes = Encoding.UTF8.GetBytes(body.Substring(0, 35));
			var key = Convert.ToBase64String(_sha256.ComputeHash(bodyBytes));
			_memoryCache.TryGetValue(key, out int currentValue);
			if (currentValue == retryCount) {
				Console.WriteLine($"{DateTime.Now:mm:ss.ffff} Return result for {key}");
				return Ok(body);
			}
			_memoryCache.Set(key, ++currentValue);
			Response.StatusCode = errorCode;
			Console.WriteLine($"{DateTime.Now:mm:ss.ffff} Return error for {key} (retry #{currentValue})");
			return BadRequest(new String('в', 10_000_000));
		}

	}
}
