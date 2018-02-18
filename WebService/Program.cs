using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WebService
{
	using System.Linq;

	public class Program
	{
		public static void Main(string[] args)
		{
			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseEnvironment(EnvironmentName.Production)
				.UseStartup<Startup>()
				.UseKestrel(options => {
					var localIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(i=>!i.IsIPv6LinkLocal);
					options.Listen(localIp, 5000);
					options.Listen(IPAddress.Loopback, 5001);
				})
				.Build();
	}
}
