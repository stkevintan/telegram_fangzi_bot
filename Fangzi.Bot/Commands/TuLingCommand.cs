using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Fangzi.Bot.Commands
{
    public class TuLingCommand : Command<TuLingCommand>
    {
        IAppConfig _config{ get; set; }

        ITelegramBotClient _bot{ get; set; }

        public TuLingCommand(IAppConfig config, ITelegramBotClient bot) {
            _config = config;
            _bot = bot;
        }
        readonly HttpClient httpClient = new HttpClient();
        async Task<string> GetReply(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                return _config.DefaultReply;
            }

            var content = JsonConvert.SerializeObject(new
            {
                reqType = 0,
                perception = new
                {
                    inputText = new
                    {
                        text = text
                    }
                },
                userInfo = new
                {
                    apiKey = _config.TulingApiKey,
                    userId = Session.Message.From.Id
                }
            });
            using var response = await httpClient.PostAsync(
                "http://openapi.tuling123.com/openapi/api/v2", new StringContent(
                    content,
                    Encoding.UTF8,
                    "application/json"
                ));
            if (!response.IsSuccessStatusCode)
            {
                return "芳子酱傻傻了";
            }

            var resContent = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ReplyJsonObject>(resContent);
            if (data.intent.code == 4003)
            {
                return "芳子酱累坏啦，明天再来吧";
            }
            var result = data.results?.Where(result => result.resultType == "text").FirstOrDefault();
            if (result == null)
            {
                return "芳子酱坏掉啦";
            }

            return result.values["text"];
        }
        public override async Task Run(string content)
        {
            var reply = await GetReply(content);
            await _bot.SendTextMessageAsync(
                chatId: Session.Message.Chat,
                text: reply
            );
        }
    }
    class Intent
    {
        public int code { get; set; }
    }

    class Result
    {
        public int groupType { get; set; }
        public string resultType { get; set; }
        public Dictionary<string, string> values { get; set; }
    }

    class ReplyJsonObject
    {
        public Intent intent { get; set; }
        public IList<Result> results { get; set; }
    }
}