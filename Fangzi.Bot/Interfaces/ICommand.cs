using System.Threading.Tasks;

namespace Fangzi.Bot.Interfaces
{
    public interface ICommand
    {
        Task RunAsync(ISession session);
	}
}