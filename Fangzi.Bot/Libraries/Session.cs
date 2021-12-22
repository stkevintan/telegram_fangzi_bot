
using Telegram.Bot.Types;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Extensions;

namespace Fangzi.Bot.Libraries
{
	public class Session : ISession
	{
		public Message Message { get; init; }

		public string? Command { get; init; }

		public string Content { get; init; }

		public Session(Message message)
		{
			Message = message;
			(Command, Content) = Message.Split();
		}
	}
}