using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Fangzi.Bot.Commands;
using Fangzi.Bot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Fangzi.Bot.Services;

namespace Fangzi.Bot.Routers
{
    public class Router
    {
        Dictionary<string, Command> _commands { get; set; }
        ITelegramBotClient _bot;
        ILogger<Router> _logger;

        IAppConfig _config;

        IServiceProvider _serviceProvider;

        public string DefaultCommandName { get; set; }

        public Router(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILogger<Router>>();
            _config = serviceProvider.GetService<IAppConfig>();
            _bot = serviceProvider.GetService<ITelegramBotClient>();
            _commands = new Dictionary<string, Command>();
        }

        public Router AddCommand(string name, Command command)
        {
            _commands.Add(name, command);
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
            Command cmd = _commands[info.Item1 ?? DefaultCommandName.ToLower()];
            if (cmd != null)
            {
                var ctx = _serviceProvider.GetService<IContext>().Open(e.Message);
                await cmd.WithContext(ctx).Run(info.Item2);
                return;
            }
            // default return
            await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: _config.DefaultReply
            );
        }

    }
}