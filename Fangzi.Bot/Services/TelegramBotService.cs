using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Fangzi.Bot.Interfaces;
using System;
using Fangzi.Bot.Attributes;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Extensions.Polling;
using System.Threading;
using Telegram.Bot.Exceptions;
using Fangzi.Bot.Libraries;

namespace Fangzi.Bot.Services
{
	public class TelegramBotService : IUpdateHandler, IBotService
	{
		IEnumerable<ICommand> _commands { get; set; }
		ILogger<TelegramBotService> _logger;
		BotConfiguration _config;
		ITelegramBotClient _bot;
		RateLimitService _rls;
		User? _me;

		public TelegramBotService(
			ILogger<TelegramBotService> logger,
			BotConfiguration config,
			ITelegramBotClient bot,
			RateLimitService rls,
			IEnumerable<ICommand> commands)
		{
			_logger = logger;
			_commands = commands;
			_rls = rls;
			_config = config;
			_bot = bot;
		}

		public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			var handler = update.Type switch
			{
				UpdateType.Message => MessageReceived(update.Message!),
				UpdateType.EditedMessage => MessageReceived(update.EditedMessage!),
				_ => Task.CompletedTask
			};
			try
			{
				await handler;
			}
			catch (Exception ex)
			{
				await HandleErrorAsync(botClient, ex, cancellationToken);
			}
		}

		public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			var ErrorMessage = exception switch
			{
				ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
				_ => exception.ToString()
			};
			_logger.LogError(ErrorMessage);
			return Task.CompletedTask;
		}

		public async Task MessageReceived(Message message)
		{
			_logger.LogTrace("Received a {1} message in chat {0};", message.Chat.Id, message.Type);

			var handler = message.Type switch
			{
				MessageType.Text => TextMessageReceived(message),
				_ => Task.CompletedTask
			};
			await handler;
		}

		public async Task TextMessageReceived(Message message)
		{
			_me = _me ?? (await _bot.GetMeAsync());

			ISession session = new Session(message, _me);
			var cmd = findCommand(session.Command);
			if (cmd is ICommand c)
			{
				try
				{
					await executeCommandAsync(c, session);
				}
				catch (Exception ex)
				{
					await _bot.SendTextMessageAsync(session.Id, "好像什么地方坏掉了QaQ");
					_logger.LogError(ex, $"Failed to run command {session.Command ?? "<Empty>"}");
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
						if (requireRole.Role == "Admin" && !isAdmin(session.Message.From!.Username!))
						{
							await _bot.SendTextMessageAsync(chatId: session.Id, text: "只有主人才能使用这条命令哦~");
							return;
						}
					}

					if (attr is RateLimitedAttribute rateLimited)
					{
						idleSec = rateLimited.Seconds ?? _config.CooldownSec;
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

		public void Run(CancellationToken token)
		{
			_bot.StartReceiving(this, cancellationToken: token);
		}
	}
}