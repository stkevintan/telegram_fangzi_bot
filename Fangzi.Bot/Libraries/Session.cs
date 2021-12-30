
using Telegram.Bot.Types;
using Fangzi.Bot.Interfaces;
using Fangzi.Bot.Extensions;
using System.Text.RegularExpressions;

namespace Fangzi.Bot.Libraries
{
	public class Session : ISession
	{
		static Regex regex = new Regex(@"^\/(\S+)(\s+(.|\n)*)?$");

		public Message Message { get; init; }

		public string? Command { get; init; }

		public string Content { get; init; }

		public Chat Chat => Message.Chat;

		public User Me { get; init; }

		public long Id => Chat.Id;

		public Session(Message message, User me)
		{
			Message = message;
			Me = me;
			(Command, Content) = split(message);
		}

		(string?, string) split(Message message)
		{
			if (message.Text is string text)
			{
				if (!text.StartsWith('/'))
				{
					return (null, text);
				}
				var match = regex.Match(text);
				if (match.Success)
				{
					var keys = match.Groups[1].Value.Split("@");
					if (keys.Length > 1 && keys[1] != Me.Username)
					{
						return (null, text);
					}
					string content = (match.Groups[2]?.Value ?? "").Trim();
					return (keys[0], content);
				}
				return (null, text);
			}
			return (null, string.Empty);
		}
	}
}