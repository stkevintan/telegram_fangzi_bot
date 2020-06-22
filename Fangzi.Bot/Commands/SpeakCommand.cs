using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Fangzi.Bot.Commands
{
    public class SpeakCommand : Command
    {
        ISpeaker _speaker { get; set; }
        IAppConfig _config { get; set; }
        public SpeakCommand(IServiceProvider container) : base(container)
        {
            _speaker = container.GetService<ISpeaker>();
            _config = container.GetService<IAppConfig>();
        }
        public override async Task Run(string content)
        {
            if (String.IsNullOrWhiteSpace(content))
            {
                await _bot.SendTextMessageAsync(chatId: _context.Message.Chat, text: "你想让我说神马来着？");
                return;
            }
            var user = _context.Message.Chat.Username;
            using var stream = await _speaker.SpeakAsync(content, _config.MasterUsers.Contains(user));
            if (stream == null)
            {
                await _bot.SendTextMessageAsync(chatId: _context.Message.Chat, text: "好像什么地方坏掉了 QaQ");
                return;
            }

            var title = content.Substring(0, Math.Min(content.Length, 8));
            await _bot.SendAudioAsync(
                chatId: _context.Message.Chat,
                audio: stream,
                title: title ?? "audio",
                performer: "fangzi",
                replyToMessageId: _context.Message.MessageId
            );
        }
    }
}