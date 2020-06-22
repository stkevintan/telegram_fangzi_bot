using Telegram.Bot.Types;

namespace Fangzi.Bot {
    public interface ISession {
        Message Message { get; set; }
    }
}