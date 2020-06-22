using System.Windows.Input;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Fangzi.Bot.Commands
{
    public abstract class Command<T> : ICommand where T : Command<T>
    {
        public string CommandName { get; private set; }

        public ISession Session { get; set; }

        public Message Message { get; set; }
        public Command()
        {
            CommandName = typeof(T).Name.Replace("Command", "").ToLower();
        }

        public ICommand Create(ISession session) {
            Session = session;
            return this;
        }
        public abstract Task Run(string content);
    }
}