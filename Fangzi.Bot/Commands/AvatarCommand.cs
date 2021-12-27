using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Fangzi.Bot.Libraries;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Attributes;
using Fangzi.Bot.Services;
using Microsoft.Extensions.Logging;

namespace Fangzi.Bot.Commands
{
	[Command("avatar", Description = "set the photo replied as group's avatar")]
	[RateLimited()]
	public class AvatarCommand : Command
	{
		ILogger<AvatarCommand> _logger;
		AvatarService _service;
		public AvatarCommand(ITelegramBotClient bot, AvatarService service, ILogger<AvatarCommand> logger) : base(bot)
		{
			_logger = logger;
			_service = service;
		}

		public async override Task RunAsync(ISession Session)
		{
			var replyMessage = Session.Message.ReplyToMessage;
			var resultTask = replyMessage?.Type switch
			{
				MessageType.Photo => _service.FromPhotoAsync(Session),
				MessageType.Sticker => _service.FromStickerAsync(Session),
				MessageType.Document => _service.FromDocumentAsync(Session),
				MessageType.ChatPhotoChanged => _service.FromChatPhotoChangedAsync(Session),
				_ => null
			};
			if (resultTask is null) {
				_logger.LogWarning($"Unsupported message type: {replyMessage?.Type}.");
				await _bot.SendTextMessageAsync(
					Session.Id,
					"不支持的消息",
					replyToMessageId: Session.Message.MessageId
				);
				return;
			}
			var result = await resultTask;
			await result.Match(
				async (error) =>
				{
					await _bot.SendTextMessageAsync
					(
						chatId: Session.Id,
						error,
						replyToMessageId: Session.Message.MessageId
					);
				},
				async (stream) =>
				{
					try
					{
						await _bot.SetChatPhotoAsync(replyMessage!.Chat.Id, stream);
					}
					finally
					{
						stream.Close();
					}
				}
			);
		}
	}
}