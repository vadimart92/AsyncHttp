using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace WebService.Controllers
{
	[Route("api/[controller]")]
	public class LoadTestController : Controller
	{
		private readonly IMemoryCache _memoryCache;

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
		public async Task<string> GetData([FromBody]string body, int retryCount, int delay) {
			await Task.Delay(TimeSpan.FromMilliseconds(delay));
			if (!_memoryCache.TryGetValue(body, out int currentValue)) {
				_memoryCache.Set(body, 1);
			}
			if (currentValue == retryCount) {
				Console.WriteLine($"Return result for {body}");
				return await Task.Run(() => Convert.ToBase64String(Encoding.UTF8.GetBytes(body)));
			}
			_memoryCache.Set(body, ++currentValue);
			Response.StatusCode = 500;
			Console.WriteLine($"Return error for {body} (retry #{currentValue})");
			return string.Empty;
		}

	}
}
