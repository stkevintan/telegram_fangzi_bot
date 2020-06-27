using System;
using System.Reflection;
using Telegram.Bot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fangzi.Bot.Routers;
using System.Linq;
using Fangzi.Bot.Commands;

namespace Fangzi.Bot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void UseTelegramBot(this IServiceCollection services)
        {
            services.AddSingleton<ITelegramBotClient>(container =>
                new TelegramBotClient(container.GetService<IAppConfig>().TelegramAccessToken)
            );
        }

        public static void UseRouter(this IServiceCollection services, string defaultRouter)
        {
            var assembly = Assembly.GetEntryAssembly();
            var ICommandType = typeof(ICommand);
            var list = assembly.GetExportedTypes().Where(t => {
                if(t.BaseType == null || !t.BaseType.IsGenericType) return false;
                return t.BaseType.GetGenericTypeDefinition()  == typeof(Command<>);
            }).ToList();
            list.ForEach(c => services.AddSingleton(ICommandType, c));

            services.AddSingleton<Router>((container) =>
            {
                return new Router(container.GetService<ILogger<Router>>(), container.GetServices<ICommand>())
                { DefaultCommandName = defaultRouter };
            });
        }
    }
}