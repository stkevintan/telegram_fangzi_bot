using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Fangzi.Bot.Services
{
    public class AppConfigService: IAppConfig
    {
        public string DefaultReply { get; }
        public string TelegramAccessToken { get; }

        public string SpeechSubscription { get; }
        public string SpeechRegion { get; }
        public string TulingApiKey { get; }
        public List<string> MasterUsers { get; }

        public AppConfigService(IConfiguration configuration)
        {
            DefaultReply = configuration[nameof(DefaultReply)] ?? "喵喵喵？？";
            TelegramAccessToken = configuration[nameof(TelegramAccessToken)];
            SpeechSubscription = configuration[nameof(SpeechSubscription)];
            SpeechRegion = configuration[nameof(SpeechRegion)];
            TulingApiKey = configuration[nameof(TulingApiKey)];
            MasterUsers = new List<string>(configuration.GetSection(nameof(MasterUsers))?.Get<string[]>() ?? new string[] { });
            validate();
        }

        private void validate()
        {
            if (string.IsNullOrEmpty(TelegramAccessToken))
            {
                throw new ArgumentNullException(nameof(TelegramAccessToken),
                 "Telegram access token is not provider or is empty.");
            }

            if (string.IsNullOrEmpty(SpeechSubscription))
            {
                throw new ArgumentNullException(nameof(SpeechSubscription),
                "speech service subscription is not provider or is empty.");
            }

            if (string.IsNullOrEmpty(SpeechRegion))
            {
                throw new ArgumentNullException(nameof(SpeechRegion),
                "speech service region is not provider or is empty.");
            }

            if (string.IsNullOrEmpty(TulingApiKey))
            {
                throw new ArgumentNullException(nameof(TulingApiKey),
                "tuling api key is not provider or is empty.");
            }
        }

    }
}
