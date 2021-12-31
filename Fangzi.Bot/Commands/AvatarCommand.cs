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
		public int? FaceIndex;
		public bool ShowFaces;

		public bool IsHelp = false;
	}

	[Command("avatar", Description = "set the photo replied as group's avatar.`-help` for advanced usages")]
	[RateLimited()]
	public class AvatarCommand : Command
	{
		static int _max_size = 15 * (1 << 20);
		static Regex colorRegex = new Regex(@"(?:\s|^)-b(?:ackground)?\s*=\s*#([0-9a-zA-Z]{3}|[0-9a-zA-Z]{6})\b", RegexOptions.IgnoreCase);
		static Regex faceRegex = new Regex(@"(?:\s|^)-f(?:ace)\s*(=\s*(\d+))?\b", RegexOptions.IgnoreCase);
		static Regex showFacesRegex = new Regex(@"(?:\s|^)-s(?:how_faces)\s*(=\s*(true|false))?\b", RegexOptions.IgnoreCase);

		static Regex helpRegex = new Regex(@"(?:\s|^)-help\s*\b", RegexOptions.IgnoreCase);

		ILogger<AvatarCommand> _logger;

		BotConfiguration _config;
		public AvatarCommand(ITelegramBotClient bot, BotConfiguration config, ILogger<AvatarCommand> logger) : base(bot)
		{
			_logger = logger;
			_config = config;
		}

		public async override Task RunAsync(ISession Session)
		{
			var args = parseArgs(Session.Content);
			if (args.IsHelp)
			{
				string markdownContent = @"Set the replied photo as the group's avatar:  
`-b[ackground]=<hex color>`: set the background color for transparent avatars
`-f[ace][=<int>]`: auto detect the anime faces, crop and center the i-th face (start from 0) as the avatar (default to center all the faces)
`-s[howFaces][=<bool>]`: mark all the anime faces detected in the given photos
";
				await _bot.SendTextMessageAsync(
					Session.Id,
					text: markdownContent,
					parseMode: ParseMode.Markdown
				);
				return;
			}
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
					"太大了,喂!",
					replyToMessageId: Session.Message.MessageId
				);
				return;
			}


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

			service.AddImageBackground(args.Background);

			if (args.ShowFaces)
			{
				service.AnimeFaceDetect(_config.AnimeFaceDetectCascadePath, crop: false).Resize();
				using var photo = service.toStream();
				await _bot.SendPhotoAsync(Session.Id, photo!);
			}
			else
			{
				if (args.FaceIndex is int index)
				{
					service.AnimeFaceDetect(_config.AnimeFaceDetectCascadePath, index: index).Resize();
				}
				using var photo = service.toStream();

				await _bot.SetChatPhotoAsync(reply!.Chat.Id, photo);
			}
		}

		AvatarArgs parseArgs(string text)
		{
			// -help | -C[olor]=<hex> | -Face[=<int>] | -ShowFaces[=<true|false>]
			if (helpRegex.Match(text).Success)
			{
				return new AvatarArgs { IsHelp = true };
			}
			var background = (255, 255, 255);
			int? faceIndex = null;
			bool showFaces = false;
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
			if (faceRet.Success)
			{
				var value = faceRet.Groups[1]?.Value;
				if (String.IsNullOrEmpty(value))
				{
					faceIndex = -1;
				}
				else
				{
					faceIndex = Convert.ToInt32(faceRet.Groups[2]!.Value);
				}
			}

			var showFaceRet = showFacesRegex.Match(text);
			if (showFaceRet.Success)
			{
				var value = showFaceRet.Groups[1]?.Value;
				if (String.IsNullOrEmpty(value))
				{
					showFaces = true;
				}
				else
				{
					string value2 = showFaceRet.Groups[2]?.Value ?? "";
					if (value2.Contains("true"))
					{
						showFaces = true;
					}
				}
			}

			return new AvatarArgs { Background = background, FaceIndex = faceIndex, ShowFaces = showFaces };
		}
	}
}