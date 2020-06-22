using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Fangzi.Bot.Commands
{
    public class SpeakCommand : Command<SpeakCommand>
    {
        ISpeaker _speaker { get; set; }
        IAppConfig _config { get; set; }
        ITelegramBotClient _bot { get; set; }

        public SpeakCommand(ITelegramBotClient bot, IAppConfig config, ISpeaker speaker)
        {
            _config = config;
            _bot = bot;
            _speaker = speaker;
        }
        public override async Task Run(string content)
        {
            if (String.IsNullOrWhiteSpace(content))
            {
                await _bot.SendTextMessageAsync(chatId: Session.Message.Chat, text: "你想让我说神马来着？");
                return;
            }
            var user = Session.Message.Chat.Username;
            using var stream = await _speaker.SpeakAsync(content, _config.MasterUsers.Contains(user));
            if (stream == null)
            {
                await _bot.SendTextMessageAsync(chatId: Session.Message.Chat, text: "好像什么地方坏掉了 QaQ");
                return;
            }

            var title = content.Substring(0, Math.Min(content.Length, 8));
            await _bot.SendAudioAsync(
                chatId: Session.Message.Chat,
                audio: stream,
                title: title ?? "audio",
                performer: "fangzi",
                replyToMessageId: Session.Message.MessageId
            );
        }
    }
}