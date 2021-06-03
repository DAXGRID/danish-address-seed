using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DanishAddressSeed
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = BuildServiceProvider();
            var startup = serviceProvider.GetService<Startup>();
            startup.Start();
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddSingleton<Startup>()
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
