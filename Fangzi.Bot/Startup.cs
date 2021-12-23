using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Services;

namespace Fangzi.Bot
{
	public class Startup : BackgroundService
	{
		readonly ILogger<Startup> _logger;
		readonly BotConfiguration _config;
		readonly IBotService _botService;
		public Startup(
			ILogger<Startup> logger,
			BotConfiguration config,
			IBotService botService
		)
		{
			_logger = logger;
			_config = config;
			_botService = botService;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Service starting");
			stoppingToken.Register(() =>
				_logger.LogInformation("Service stoping, bye~"));

			while (!stoppingToken.IsCancellationRequested)
			{
				_botService.Run(stoppingToken);
				await Task.Delay(Timeout.Infinite, stoppingToken);
			}
		}
	}
}