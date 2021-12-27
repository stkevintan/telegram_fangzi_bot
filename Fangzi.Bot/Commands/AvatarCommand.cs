using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Fangzi.Bot.Libraries;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Attributes;
using Fangzi.Bot.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace Fangzi.Bot.Commands
{
	public record AvatarArgs
	{
		public (int, int, int) Background;
		public int FaceCount;
	}

	[Command("avatar", Description = "set the photo replied as group's avatar")]
	[RateLimited()]
	public class AvatarCommand : Command
	{
		static int _max_size = 15 * (1 << 20);
		static Regex colorRegex = new Regex(@"(?:\s|^)\/c(?:olor)?\s*=\s*#([0-9a-zA-Z]{3}|[0-9a-zA-Z]{6})\b");
		static Regex faceRegex = new Regex(@"(?:\s|^)\/face\s*=\s*(\d+)\b");

		ILogger<AvatarCommand> _logger;
		public AvatarCommand(ITelegramBotClient bot, ILogger<AvatarCommand> logger) : base(bot)
		{
			_logger = logger;
		}

		public async override Task RunAsync(ISession Session)
		{
			var reply = Session.Message.ReplyToMessage;
			FileBase? file = reply?.Type switch
			{
				MessageType.Photo => reply.Photo?.MaxBy(p => p.FileSize),
				MessageType.Sticker => reply.Sticker,
				MessageType.Document => reply.Document,
				MessageType.ChatPhotoChanged => reply.NewChatPhoto?.MaxBy(p => p.FileSize),
				_ => null
			};

			if (file is null)
			{
				_logger.LogWarning($"Unsupported message type({reply?.Type}) or no image found.");
				await _bot.SendTextMessageAsync(
					Session.Id,
					"图呢？？？",
					replyToMessageId: Session.Message.MessageId
				);
				return;
			}

			if (file.FileSize > _max_size)
			{
				_logger.LogWarning($"File size to large: {file.FileSize}");
				await _bot.SendTextMessageAsync(
					Session.Id,
					"图太大了 >_<",
					replyToMessageId: Session.Message.MessageId
				);
				return;
			}

			var args = parseArgs(Session.Content);

			using var stream = new MemoryStream();
			await _bot.GetInfoAndDownloadFileAsync(file.FileId, stream);
			using var service = AvatarService.FromStream(stream);

			if (service is null)
			{
				_logger.LogWarning($"File is not supported.");
				await _bot.SendTextMessageAsync(
					Session.Id,
					"文档/图片格式不支持 >_<",
					replyToMessageId: Session.Message.MessageId
				);
				return;
			}

			using var photo = service
				.AddImageBackground(args.Background)
				.Resize()
				.toStream();

			await _bot.SetChatPhotoAsync(reply!.Chat.Id, photo);
		}

		AvatarArgs parseArgs(string text)
		{
			// /c[olor]=<hex> /face=<int>
			var background = (255, 255, 255);
			var faceCount = 0;
			var colorRet = colorRegex.Match(text);
			if (colorRet.Success && colorRet.Groups[1] != null)
			{
				var hex = colorRet.Groups[1]!.Value;
				var indexer = hex.Length switch
				{
					3 => new int[,] { { 0, 0 }, { 1, 1 }, { 2, 2 } },
					_ => new int[,] { { 0, 1 }, { 2, 3 }, { 4, 5 } }
				};
				var c3 = Enumerable.Range(0, 3)
					.Select(i => (indexer[i, 0], indexer[i, 1]))
					.Select(p => $"{hex[p.Item1]}{hex[p.Item2]}")
					.Select(s => Convert.ToInt32(s, 16))
					.ToList();

				background = (c3[0], c3[1], c3[2]);
			}
			var faceRet = faceRegex.Match(text);
			if (faceRet.Success && faceRet.Groups[1] != null)
			{
				faceCount = Convert.ToInt32(faceRet.Groups[1]!.Value);
			}
			return new AvatarArgs { Background = background, FaceCount = faceCount };
		}
	}
}