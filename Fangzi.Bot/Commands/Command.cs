using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Fangzi.Bot.Commands
{
    public abstract class Command
    {
        public string CommandName { get; set; }

        protected IContext _context { get; set; }


        IServiceProvider _container { get; set; }

        protected ITelegramBotClient _bot  { get; set; }
        public Command(IServiceProvider container) {
            _container = container;
            _bot = container.GetService<ITelegramBotClient>();
        }

        public Command WithContext(IContext context)
        {
            _context = context;
            return this;
        }
        public abstract Task Run(string content);
    }
}