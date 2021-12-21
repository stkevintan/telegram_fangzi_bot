using Telegram.Bot;
using System.Threading.Tasks;

namespace Fangzi.Bot.Commands
{
    public class EchoCommand : Command<EchoCommand>
    {

        ITelegramBotClient _bot { get; set; }
        public EchoCommand(ITelegramBotClient bot)
        {
            _bot = bot;
        }
        public override async Task Run(string content)
        {
            await _bot.SendTextMessageAsync(
                chatId: Session.Message.Chat,
                text: content + "?"
            );
        }
    }
}