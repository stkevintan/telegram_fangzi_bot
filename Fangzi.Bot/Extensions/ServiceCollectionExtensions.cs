using System.Reflection;
using Telegram.Bot;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Libraries;
using Fangzi.Bot.Attributes;
using Fangzi.Bot.Services;

namespace Fangzi.Bot.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static void UseTelegramBot(this IServiceCollection services)
		{
			services.AddSingleton<ITelegramBotClient>(container =>
				new TelegramBotClient(container.GetService<BotConfiguration>()!.TelegramAccessToken)
			);
			var assembly = Assembly.GetEntryAssembly();
			Assert.NotNull(assembly);
			var ICommandType = typeof(ICommand);
			var list = assembly.GetExportedTypes().Where(t =>
			{
				if (t.BaseType == typeof(Command))
				{
					// only commands prefix with CommandAttribute
					return System.Attribute.GetCustomAttribute(t, typeof(CommandAttribute)) != null;
				}
				return false;
			}).ToList();
			list.ForEach(c => services.AddSingleton(ICommandType, c));
			services.AddSingleton<IBotService, TelegramBotService>();
		}
	}
}