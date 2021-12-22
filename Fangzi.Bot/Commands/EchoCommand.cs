using Telegram.Bot;
using System.Threading.Tasks;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Libraries;
using Fangzi.Bot.Attributes;

namespace Fangzi.Bot.Commands
{
	[Command("echo", Description = "echo the same text with a question mark")]
	public class EchoCommand : Command
	{
		public EchoCommand(ITelegramBotClient bot) : base(bot)
		{
		}

		public override async Task RunAsync(ISession Session)
		{
			await _bot.SendTextMessageAsync(
				chatId: Session.Id,
				text: Session.Content + "?"
			);
		}
	}
}