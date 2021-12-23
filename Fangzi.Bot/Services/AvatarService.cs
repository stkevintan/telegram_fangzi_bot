using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Linq;
using System.IO;
using OpenCvSharp;

namespace Fangzi.Bot.Services
{

	public record class AvatarResult
	{
		public string? ErrorMessage;
		public string? FileLocation;

		public Task Match(Func<string, Task> onError, Func<string, Task> onOk)
		{
			if (ErrorMessage is string message)
			{
				return onError(message);
			}
			return onOk(FileLocation!);
		}
	}

	public class AvatarService
	{
		int _max_size = 15 * (1 << 11);

		ITelegramBotClient _bot;

		public AvatarService(ITelegramBotClient bot)
		{
			_bot = bot;
		}

		public async Task<AvatarResult> FromPhotoAsync(Message message)
		{
			var photo = message.Photo?.MaxBy(p => p.FileSize);
			if (photo is null)
			{
				return new AvatarResult { ErrorMessage = "图呢？？？" };
			}
			if (photo.FileSize > _max_size)
			{
				return new AvatarResult { ErrorMessage = "不要啊，太大了！！！" };
			}

			return new AvatarResult
			{
				FileLocation = await downloadImage(photo.FileId, message.Chat.Id)
			};
		}

		public async Task<AvatarResult> FromStickerAsync(Message message)
		{
			var sticker = message.Sticker;
			if (sticker is null)
			{
				return new AvatarResult { ErrorMessage = "图呢？？？" };
			}
			return new AvatarResult
			{
				FileLocation = await downloadImage(sticker.FileId, message.Chat.Id, nameof(sticker))
			};
		}

		string getTmpPath(string prefix, long chatId)
		{
			return Path.Join(Path.GetTempPath(), $"{prefix}{chatId}.jpg");
		}

		async Task<string> downloadImage(string FileId, long chatId, string prefix = "image")
		{
			var FileLocation = getTmpPath(prefix, chatId);
			using (var stream = System.IO.File.Create(FileLocation))
			{
				await _bot.GetInfoAndDownloadFileAsync(FileId, stream);
			}
			resize(FileLocation);
			return FileLocation;
		}

		void resize(string FileLocation)
		{
			using var src = new Mat(FileLocation, ImreadModes.Unchanged);
			using var dst = new Mat();
			var rows = src.Rows;
			var cols = src.Cols;
			var channels = src.Channels;
			var scaleFactor = 2048.0 / Math.Min(rows, cols);
			Cv2.Resize(
				src, 
				dst,
				new Size(rows * scaleFactor, cols * scaleFactor), 
				fx: scaleFactor, 
				fy: scaleFactor, 
				interpolation: InterpolationFlags.Area
			);
			dst.ImWrite(FileLocation);
		}


	}
}