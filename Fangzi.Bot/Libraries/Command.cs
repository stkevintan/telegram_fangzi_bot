using System.Threading.Tasks;
using Fangzi.Bot.Interfaces;
using Telegram.Bot;

namespace Fangzi.Bot.Libraries
{
    // TODO: remove it.
    public abstract class Command : ICommand
    {
        protected ITelegramBotClient _bot { get; set; }

        public Command(ITelegramBotClient bot) {
            _bot = bot;
        }

        public abstract Task RunAsync(ISession session);
	}
}