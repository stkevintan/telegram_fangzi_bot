using System;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Fangzi.Bot.Routers;

namespace Fangzi.Bot.Extensions
{
    public static class TelegramBotClientExtensions
    {
        static Regex regex = new Regex(@"^\/(\w+)(\s+(.|\n)*)?$");
        public static void AddRouting(this ITelegramBotClient bot, Router router)
        {
            bot.OnMessage += router.BotOnMessageReceived;
        }

        public static Tuple<string, string> Split(this Message message)
        {
            var match = regex.Match(message.Text);
            if (match.Success)
            {
                string key = match.Groups[1].Value;
                string content = (match.Groups[2]?.Value ?? "").Trim();
                return Tuple.Create(key, content);
            }
            return Tuple.Create<string,string>(null, message.Text);
        }
    }
}