using Telegram.Bot;
using System.Threading.Tasks;

namespace Fangzi.Bot.Commands
{
    public class HelloCommand : Command<HelloCommand>
    {

        ITelegramBotClient _bot { get; set; }
        public HelloCommand(ITelegramBotClient bot)
        {
            _bot = bot;
        }
        public override async Task Run(string content)
        {
            await _bot.SendTextMessageAsync(
                chatId: Session.Message.Chat,
                text: "你好哇~"
            );
        }
    }
}