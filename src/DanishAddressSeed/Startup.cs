using Microsoft.Extensions.Logging;

namespace DanishAddressSeed
{
    internal class Startup
    {
        private readonly ILogger<Startup> _logger;

        public Startup(ILogger<Startup> logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            _logger.LogInformation($"Starting application");
        }
    }
}
