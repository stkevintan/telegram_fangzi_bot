using Telegram.Bot;
using System.Threading.Tasks;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Libraries;

namespace Fangzi.Bot.Commands
{
    public class HelloCommand : Command<HelloCommand>
    {

        ITelegramBotClient _bot { get; set; }
        public HelloCommand(ITelegramBotClient bot)
        {
            _bot = bot;
        }
        public override async Task RunAsync(ISession Session)
        {
            await _bot.SendTextMessageAsync(
                chatId: Session.Id,
                text: "你好哇~"
            );
        }
    }
}