using System.Linq;
using System.Collections.Generic;
using Fangzi.Bot.Extensions;
using Telegram.Bot.Args;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Fangzi.Bot.Commands;

namespace Fangzi.Bot.Routers
{
    public class Router
    {
        IEnumerable<ICommand> _commands { get; set; }
        ILogger<Router> _logger;


        public string DefaultCommandName { get; set; }

        public Router(ILogger<Router> logger, IEnumerable<ICommand> commands)
        {
            _logger = logger;
            _commands = commands;
        }

        public Router AddCommand(ICommand command)
        {
            _commands.Append(command);
            return this;
        }

        public async void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message.Type != MessageType.Text)
            {
                _logger.LogWarning("Message Type {0} not supported", message.Type);
                return;
            }
            _logger.LogTrace("Received a text message in chat {0};", message.Chat.Id);
            var info = message.Split();
            var cmdName = info.Item1 ?? DefaultCommandName.ToLower();
            var cmd = _commands.FirstOrDefault(c => c.CommandName == cmdName);
            if (cmd != null)
            {
                await cmd.Create(new Session(message)).Run(info.Item2);
                return;
            }
            // default return
            // await _bot.SendTextMessageAsync(
            //     chatId: message.Chat.Id,
            //     text: _config.DefaultReply
            // );
        }

    }
}