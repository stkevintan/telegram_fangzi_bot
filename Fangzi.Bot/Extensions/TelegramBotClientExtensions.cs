using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace Fangzi.Bot.Extensions;
public static class TelegramBotClientExtensions
{
    static Regex regex = new Regex(@"^\/(\S+)(\s+(.|\n)*)?$");

    public static (string?, string) Split(this Message message)
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
                if (keys.Length > 1 && keys[1] != "fangzi_bot")
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