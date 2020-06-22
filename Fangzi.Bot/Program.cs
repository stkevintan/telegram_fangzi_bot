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
            // .ConfigureAppConfiguration((context, builder) =>
            // {
            //     builder.SetBasePath(Directory.GetCurrentDirectory());
            //     builder.AddJsonFile("appsetings.json", optional: true);
            //     builder.AddCommandLine(args);
            // })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IAppConfig, AppConfigService>();
                services.UseTelegramBot();
                services.AddTransient<IContext, ContextService>();
                services.AddSingleton<ISpeaker, SpeakerService>();
                services.UserRouter();
                services.AddHostedService<Startup>();
            });
    }
}
