using Telegram.Bot.Types;

namespace Fangzi.Bot.Interfaces {
    public interface ISession {
        Message Message { get; set; }
    }
}