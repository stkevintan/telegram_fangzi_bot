using Telegram.Bot;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.IO;
using System.Linq;

namespace Fangzi.Bot.Commands
{
	public class ChatStatus {
		public bool running {set; get;}
		public DateTime endTime {set; get; }
	}

    public class AvatarCommand: Command<AvatarCommand>
    {
		private Dictionary<long, ChatStatus> chats = new Dictionary<long, ChatStatus>();
        ITelegramBotClient _bot { get; set; }
        public AvatarCommand(ITelegramBotClient bot)
        {
            _bot = bot;
        }
        public override async Task Run(string content)
        {
			var reply = Session.Message.ReplyToMessage;
			var Id = Session.Message.Chat.Id;
			if(!checkChat(Id)) {
				await _bot.SendTextMessageAsync(
					chatId: Id,
					text: "贤者模式"
				);
			}

			try {
				await getImageAsync(reply);
			} finally {
				chats[Id].running = false;
				chats[Id].endTime = DateTime.Now;
			}
		}

		private bool checkChat(long Id) {
			if(!chats.ContainsKey(Id)) {
				chats[Id] = new ChatStatus{ running = true };
				return true;
			}
			if (chats[Id].running) {
				return false;
			}
			if (chats[Id].endTime != null && (DateTime.Now - chats[Id].endTime).TotalSeconds <= 5) {
				return false;
			} 
			return true;
		}

		private async Task<string> getImageAsync(Message reply)
		{
			if(null == reply || reply.Type != MessageType.Photo) {
				return "请回复一个图片";
			}
			// var photo = reply.Photo[0];
			var photo = reply.Photo.OrderByDescending(p => p.FileSize).First();
			Console.WriteLine($"{photo.Height} {photo.Width}");
			if(photo.FileSize > 15* Math.Pow(1024, 2)) {
				return "不要啊啊啊啊，太大了！！！";
			}
			var filePath = Path.Join(Path.GetTempPath(), $"image{reply.Chat.Id}.tmp");
			Console.WriteLine(filePath);
			using (var stream = System.IO.File.Create(filePath)) {
				await _bot.GetInfoAndDownloadFileAsync(photo.FileId, stream);
			}
			using (var stream = System.IO.File.OpenRead(filePath)) {
				await _bot.SetChatPhotoAsync(reply.Chat.Id, stream);
				await _bot.SetChatDescriptionAsync(reply.Chat.Id, DateTime.Now + "@test");
			}
			Console.WriteLine("Done");

			System.IO.File.Delete(filePath);
			return null;
		}
    }
}