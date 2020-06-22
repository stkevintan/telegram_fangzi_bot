using Telegram.Bot.Types;
using Telegram.Bot;
namespace Fangzi.Bot.Services
{
    public class ContextService: IContext
    {
        public long Id { get; set; }
        public Message Message { get; set; }

        public IContext Open(Message message)
        {
            Message = message;
            Id = message.Chat.Id;
            return this;
        }

    }
}