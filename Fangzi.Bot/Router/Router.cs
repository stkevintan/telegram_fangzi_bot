using System.Linq;
using System.Collections.Generic;
using Telegram.Bot.Args;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Fangzi.Bot.Interfaces;
using System;
using Fangzi.Bot.Libraries;
using Fangzi.Bot.Attributes;
using System.Threading.Tasks;
using Telegram.Bot;
using Fangzi.Bot.Services;

namespace Fangzi.Bot.Routers
{
	public class Router
	{
		IEnumerable<ICommand> _commands { get; set; }
		ILogger<Router> _logger;
		IAppConfig _config;
		ITelegramBotClient _bot;
		RateLimitService _rls;

		public Router(ILogger<Router> logger, IAppConfig config, ITelegramBotClient bot, RateLimitService rls, IEnumerable<ICommand> commands)
		{
			_logger = logger;
			_commands = commands;
			_rls = rls;
			_config = config;
			_bot = bot;
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
			var cmd = findCommand(session.Command);
			if (cmd is ICommand c)
			{
				try
				{
					await executeCommandAsync(c, session);
				}
				catch (Exception ex)
				{
					_logger.LogTrace(ex, $"Failed to run command {session.Command ?? "<Empty>"}");
				}
			}
		}

		ICommand? findCommand(string? name)
		{
			if (name is string n)
			{
				return _commands.FirstOrDefault(c =>
				{
					var attr = System.Attribute.GetCustomAttribute(c.GetType(), typeof(CommandAttribute));
					if (attr is CommandAttribute cmd)
					{
						return cmd.Name == name;
					}
					return false;
				});
			}
			return null;
		}

		async Task executeCommandAsync(ICommand cmd, ISession session)
		{
			int idleSec = -1;
			try
			{
				var attrs = System.Attribute.GetCustomAttributes(cmd.GetType());
				foreach (var attr in attrs)
				{
					if (attr is RequireRoleAttribute requireRole)
					{
						if (requireRole.Role == "Admin" && !isAdmin(session.Message.From.Username))
						{
							await _bot.SendTextMessageAsync(chatId: session.Id, text: "只有主人才能使用这条命令哦~");
							return;
						}
					}

					if (attr is RateLimitedAttribute rateLimited)
					{
						idleSec = rateLimited.Seconds ?? _config.RateIdleSec;
						(bool ok, string err) = _rls.BeforeCheck(session.Id);
						if (!ok)
						{
							idleSec = -1;
							await _bot.SendTextMessageAsync(chatId: session.Id, text: err);
							return;
						}
					}
				}
				await cmd.RunAsync(session);
			}
			finally
			{
				if (idleSec >= 0)
				{
					_rls.AfterDone(session.Id, idleSec);
				}
			}
		}

		bool isAdmin(string username)
		{
			return _config.AdminUsers.Contains(username);
		}
	}
}