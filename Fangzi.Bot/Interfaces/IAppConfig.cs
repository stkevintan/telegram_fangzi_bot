using System.Collections.Generic;
namespace Fangzi.Bot.Interfaces
{
	public interface IAppConfig
	{
		public string TelegramAccessToken { get; }
		public string SpeechSubscription { get; }
		public string SpeechRegion { get; }
		public List<string> AdminUsers { get; }

		public int RateIdleSec { get; }

	}
}