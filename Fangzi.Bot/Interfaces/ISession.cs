using Telegram.Bot.Types;

namespace Fangzi.Bot.Interfaces
{
	public interface ISession
	{
		Message Message { get; }

		Chat Chat { get; }

		long Id { get; }

		User Me { get; }

		string? Command { get; }

		string Content { get; }
	}
}