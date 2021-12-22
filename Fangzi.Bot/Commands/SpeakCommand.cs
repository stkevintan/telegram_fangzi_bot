using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Libraries;

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
        public override async Task RunAsync(ISession Session)
        {
            if (String.IsNullOrWhiteSpace(Session.Content))
            {
                await _bot.SendTextMessageAsync(chatId: Session.Message.Chat, text: "你想让我说神马来着？");
                return;
            }
            var user = Session.Chat.Username;
            using var stream = await _speaker.SpeakAsync(Session.Content);
            if (stream == null)
            {
                await _bot.SendTextMessageAsync(chatId: Session.Id, text: "好像什么地方坏掉了 QaQ");
                return;
            }

            var title = Session.Content.Substring(0, Math.Min(Session.Content.Length, 8));
            await _bot.SendAudioAsync(
                chatId: Session.Id,
                audio: stream,
                title: title ?? "audio",
                performer: "fangzi",
                replyToMessageId: Session.Message.MessageId
            );
        }
    }
}