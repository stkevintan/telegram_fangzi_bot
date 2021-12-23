using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Routers;

namespace Fangzi.Bot
{
	public class Startup : BackgroundService
	{
		readonly ILogger<Startup> _logger;
		readonly IAppConfig _config;
		readonly Router _router;
		readonly ITelegramBotClient _bot;
		public Startup(
			ILogger<Startup> logger,
			IAppConfig config,
			Router router,
			ITelegramBotClient client)
		{
			_logger = logger;
			_config = config;
			_bot = client;
			_router = router;
			_logger.LogInformation("telegram client id: {0}", client.BotId);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Service starting");
			stoppingToken.Register(() =>
				_logger.LogInformation("Service stoping, bye~"));

			while (!stoppingToken.IsCancellationRequested)
			{
				_bot.StartReceiving(_router, cancellationToken: stoppingToken);
				await Task.Delay(Timeout.Infinite, stoppingToken);
			}
		}
	}
}