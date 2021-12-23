using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Fangzi.Bot.Libraries;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Attributes;
using Fangzi.Bot.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;

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
				MessageType.Photo => _service.FromPhotoAsync(replyMessage),
				MessageType.Sticker => _service.FromStickerAsync(replyMessage),
				_ => Task.FromResult(new AvatarResult { ErrorMessage = $"不支持的消息类型: {replyMessage?.Type}" })
			};
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
				async (fileLocation) =>
				{
					try
					{
						using (var stream = System.IO.File.OpenRead(result.FileLocation!))
						{
							await _bot.SetChatPhotoAsync(replyMessage!.Chat.Id, stream);
						}
					}
					finally
					{
						System.IO.File.Delete(result.FileLocation!);
					}
				}
			);
		}
	}
}