using System.Threading.Tasks;
using Fangzi.Bot.Interfaces;

namespace Fangzi.Bot.Libraries
{
    public abstract class Command<T> : ICommand where T : Command<T>
    {
        public string CommandName { get; private set; }

        public Command()
        {
            CommandName = typeof(T).Name.Replace("Command", "").ToLower();
        }

		public abstract Task RunAsync(ISession session);
	}
}