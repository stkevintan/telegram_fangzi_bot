using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fangzi.Bot.Extensions;
using Fangzi.Bot.Services;

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
                services.AddSingleton<IAppConfig, AppConfigService>();
                services.UseTelegramBot();
                services.AddSingleton<ISpeaker, SpeakerService>();
                services.UserRouter("TuLing");
                services.AddHostedService<Startup>();
            });
    }
}
