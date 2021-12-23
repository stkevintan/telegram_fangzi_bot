using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Libraries;
using Fangzi.Bot.Attributes;

namespace Fangzi.Bot.Commands
{
	[Command("speak", Description = "speak given text out")]
	[RateLimited()]
	public class SpeakCommand : Command
	{
		ISpeaker _speaker { get; set; }

		public SpeakCommand(ITelegramBotClient bot, ISpeaker speaker) : base(bot)
		{
			_speaker = speaker;
		}

		public override async Task RunAsync(ISession Session)
		{
			if (String.IsNullOrWhiteSpace(Session.Content))
			{
				await _bot.SendTextMessageAsync(chatId: Session.Id, text: "你想让我说神马来着？");
				return;
			}
			using var stream = await _speaker.SpeakAsync(Session.Content);
			if (stream == null)
			{
				await _bot.SendTextMessageAsync(chatId: Session.Id, text: "好像什么地方坏掉了 QaQ");
				return;
			}

			var title = Session.Content.Substring(0, Math.Min(Session.Content.Length, 8));
			await _bot.SendAudioAsync(
				chatId: Session.Id,
				audio: stream!,
				title: title ?? "audio",
				performer: "fangzi",
				replyToMessageId: Session.Message.MessageId
			);
		}
	}
}