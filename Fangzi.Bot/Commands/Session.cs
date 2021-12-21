
using Telegram.Bot.Types;
using Fangzi.Bot.Interfaces;

namespace Fangzi.Bot.Commands
{
    public class Session : ISession
    {
        public Message Message { get; set; }

        public Session(Message message)
        {
            Message = message;
        }

    }
}