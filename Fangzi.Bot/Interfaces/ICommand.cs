using System.Threading.Tasks;
using Fangzi.Bot.Libraries;

namespace Fangzi.Bot.Interfaces
{
    public interface ICommand
    {
        string CommandName { get; }
        Task RunAsync(ISession session);
	}
}