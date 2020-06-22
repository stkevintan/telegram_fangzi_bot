using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Fangzi.Bot.Services;
using Fangzi.Bot.Routers;
using Fangzi.Bot.Extensions;

namespace Fangzi.Bot
{
    public class Startup : BackgroundService
    {
        readonly ILogger<Startup> _logger;
        readonly IAppConfig _config;

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
            _bot.AddRouting(router);
            _logger.LogInformation("telegram client id: {0}", client.BotId);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service starting");
            stoppingToken.Register(() =>
                _logger.LogInformation("Service stoping, bye~"));

            while (!stoppingToken.IsCancellationRequested)
            {
                _bot.StartReceiving();
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
    }
}