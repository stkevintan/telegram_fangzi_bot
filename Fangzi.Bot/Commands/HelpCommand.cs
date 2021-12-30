using Telegram.Bot;
using System.Threading.Tasks;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Libraries;
using System.Linq;
using Fangzi.Bot.Attributes;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Fangzi.Bot.Commands
{
	[Command("help", Hidden = true)]
	public class HelpCommand : Command
	{
		public HelpCommand(ITelegramBotClient bot) : base(bot)
		{
		}

		public override async Task RunAsync(ISession Session)
		{
			var assembly = Assembly.GetEntryAssembly();
			Assert.NotNull(assembly);
			var ICommandType = typeof(ICommand);
			var cmdIntros = assembly.GetExportedTypes()
				.Select(t =>
				{
					if (t.BaseType == typeof(Command))
					{
						// only commands prefix with CommandAttribute
						return (CommandAttribute?)System.Attribute.GetCustomAttribute(t, typeof(CommandAttribute));
					}
					return null;
				})
				.Where(c => c != null && !c.Hidden)
				.Select(c => (c!.Name, (c.Description == string.Empty ? "No description" : c.Description)))
				.Select((pair) => $"{pair.Item1} - {pair.Item2}")
				.ToList();
			await _bot.SendTextMessageAsync(chatId: Session.Id, text: String.Join("\n", cmdIntros));
		}
	}
}