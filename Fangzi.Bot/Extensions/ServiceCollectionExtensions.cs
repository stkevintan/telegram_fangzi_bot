using System;
using System.Reflection;
using Telegram.Bot;
using Microsoft.Extensions.DependencyInjection;
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

        public static void UserRouter(this IServiceCollection services)
        {
            services.AddSingleton(container =>
            {
                var router = new Router(container) {DefaultCommandName = "tuling"};
                var assembly = Assembly.GetEntryAssembly();
                var list = assembly.GetExportedTypes().Where(t => t.BaseType == typeof(Command)).ToList();
                list.ForEach(c =>
                {
                    var name = c.Name.Replace("Command", "").ToLower();
                    var commandInstance = (Command)Activator.CreateInstance(c, container);
                    commandInstance.CommandName = name;
                    router.AddCommand(name, commandInstance);
                });
                return router;
            });
        }
    }
}