using System.Linq;
using System.Collections.Generic;
using Telegram.Bot.Args;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Fangzi.Bot.Interfaces;
using System;
using Fangzi.Bot.Libraries;

namespace Fangzi.Bot.Routers
{
	public class Router
	{
		IEnumerable<ICommand> _commands { get; set; }
		ILogger<Router> _logger;

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
			var session = new Session(message);
			var cmd = _commands.FirstOrDefault(c => c.CommandName == session.Command);
			if (cmd != null)
			{
				try
				{
					await cmd.RunAsync(session);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}
	}
}