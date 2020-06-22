using Telegram.Bot.Types;
namespace Fangzi.Bot
{
    public interface IContext
    {
        public long Id { get; set; }

        public Message Message { get; set; }
        public IContext Open(Message message);

    }
}