using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fangzi.Bot.Extensions;
using Fangzi.Bot.Services;
using Fangzi.Bot.Interfaces;

namespace Fangzi.Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<BotConfiguration>();
                services.AddSingleton<AvatarService>();
                services.AddSingleton<RateLimitService>();
                services.AddSingleton<ISpeaker, SpeakerService>();
                services.UseTelegramBot();
                services.AddHostedService<Startup>();
            });
    }
}
