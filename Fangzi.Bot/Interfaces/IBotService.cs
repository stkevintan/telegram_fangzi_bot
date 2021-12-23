using System.Threading;

namespace Fangzi.Bot.Interfaces
{
    public interface IBotService
    {
        void Run(CancellationToken token);
	}
}