using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Fangzi.Bot
{
    public interface ICommand
    {
        string CommandName { get; }
        ISession Session { get; set; }
        ICommand Create(ISession session);

        Task Run(string content);
    }
}