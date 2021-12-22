using Telegram.Bot.Types;

namespace Fangzi.Bot.Interfaces
{
	public interface ISession
	{
		Message Message { get; }

		Chat Chat => Message.Chat;

		long Id => Chat.Id;

		string? Command { get; }

		string Content { get; }
	}
}