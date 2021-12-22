using Telegram.Bot;
using System.Threading.Tasks;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Libraries;

namespace Fangzi.Bot.Commands
{
    public class EchoCommand : Command<EchoCommand>
    {
        private ITelegramBotClient _bot { get; set; }
        public EchoCommand(ITelegramBotClient bot)
        {
            _bot = bot;
        }
        public override async Task RunAsync(ISession Session)
        {
            await _bot.SendTextMessageAsync(
                chatId: Session.Id,
                text: Session.Content + "?"
            );
        }
    }
}