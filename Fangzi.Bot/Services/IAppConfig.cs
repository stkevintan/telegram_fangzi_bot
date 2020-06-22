using System.Collections.Generic;
namespace Fangzi.Bot
{
    public interface IAppConfig
    {
        public string DefaultReply { get; }
        public string TelegramAccessToken { get; }

        public string SpeechSubscription { get; }
        public string SpeechRegion { get; }
        public string TulingApiKey { get; }
        public List<string> MasterUsers { get; }

    }
}