using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Fangzi.Bot.Routers;

namespace Fangzi.Bot.Extensions
{
	public static class TelegramBotClientExtensions
	{
		static Regex regex = new Regex(@"^\/(\S+)(\s+(.|\n)*)?$");
		public static void AddRouting(this ITelegramBotClient bot, Router router)
		{
			bot.OnMessage += router.BotOnMessageReceived;
		}

		public static (string?, string) Split(this Message message)
		{
			if(!message.Text.StartsWith('/')) {
				return (null, message.Text);
			}
			var match = regex.Match(message.Text);
			if (match.Success)
			{
				var keys = match.Groups[1].Value.Split("@");
				if (keys.Length > 1 && keys[1] != "fangzi_bot") {
					return (null, message.Text);
				}
				string content = (match.Groups[2]?.Value ?? "").Trim();
				return (keys[0], content);
			}
			return (null, message.Text);
		}
	}
}