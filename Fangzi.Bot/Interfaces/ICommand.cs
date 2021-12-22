using System.Threading.Tasks;
using Fangzi.Bot.Libraries;

namespace Fangzi.Bot.Interfaces
{
    public interface ICommand
    {
        Task RunAsync(ISession session);
	}
}