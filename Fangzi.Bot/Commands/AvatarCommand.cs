using Telegram.Bot;
using System.Threading.Tasks;
using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.IO;
using System.Linq;
using Fangzi.Bot.Libraries;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Attributes;

namespace Fangzi.Bot.Commands
{
	[Command("avatar", Description = "set the photo replied as group's avatar")]
	[RateLimited()]
	public class AvatarCommand : Command
	{
		public AvatarCommand(ITelegramBotClient bot) : base(bot)
		{
		}

		public async override Task RunAsync(ISession Session)
		{
			var reply = Session.Message.ReplyToMessage;
			await (reply switch
			{
				Message => getImageAsync(reply),
				_ => Task.CompletedTask
			});
		}

		private async Task<string?> getImageAsync(Message reply)
		{
			if (null == reply || reply.Type != MessageType.Photo)
			{
				return "请回复一个图片";
			}
			// var photo = reply.Photo[0];
			var photo = reply.Photo?.OrderByDescending(p => p.FileSize).First();
			if (photo.FileSize > 15 * Math.Pow(1024, 2))
			{
				return "不要啊啊啊啊，太大了！！！";
			}
			var filePath = Path.Join(Path.GetTempPath(), $"image{reply.Chat.Id}.tmp");
			Console.WriteLine(filePath);
			using (var stream = System.IO.File.Create(filePath))
			{
				await _bot.GetInfoAndDownloadFileAsync(photo.FileId, stream);
			}
			using (var stream = System.IO.File.OpenRead(filePath))
			{
				await _bot.SetChatPhotoAsync(reply.Chat.Id, stream);
			}
			Console.WriteLine("Done");

			System.IO.File.Delete(filePath);
			return null;
		}
	}
}