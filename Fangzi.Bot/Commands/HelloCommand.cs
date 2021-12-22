using Telegram.Bot;
using System.Threading.Tasks;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Libraries;
using Fangzi.Bot.Attributes;

namespace Fangzi.Bot.Commands
{
	[Command("hello", Description = "say hello")]
	public class HelloCommand : Command
	{
		public HelloCommand(ITelegramBotClient bot) : base(bot)
		{
		}

		public override async Task RunAsync(ISession Session)
		{
			await _bot.SendTextMessageAsync(
				chatId: Session.Id,
				text: "你好哇~"
			);
		}
	}
}