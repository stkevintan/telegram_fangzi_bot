using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Fangzi.Bot;

public class BotConfiguration
{
	public string TelegramAccessToken { get; }
	public string SpeechSubscription { get; }
	public string SpeechRegion { get; }
	public List<string> AdminUsers { get; }

	public string AnimeFaceDetectCascadePath { get; }

	public int CooldownSec { get; }

	public BotConfiguration(IConfiguration configuration)
	{
		TelegramAccessToken = configuration[nameof(TelegramAccessToken)];
		SpeechSubscription = configuration[nameof(SpeechSubscription)];
		SpeechRegion = configuration[nameof(SpeechRegion)];
		AdminUsers = new List<string>(configuration.GetSection(nameof(AdminUsers))?.Get<string[]>() ?? new string[] { });
		CooldownSec = configuration.GetValue<int?>(nameof(CooldownSec)) ?? 10;
		AnimeFaceDetectCascadePath = configuration[nameof(AnimeFaceDetectCascadePath)];
		if (AnimeFaceDetectCascadePath is string p && !Path.IsPathRooted(p))
		{
			AnimeFaceDetectCascadePath = Path.GetFullPath(p);
		}
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
		if (CooldownSec < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(CooldownSec), "rate idle seconds should not be less than 0");
		}

		if (string.IsNullOrEmpty(AnimeFaceDetectCascadePath))
		{
			throw new ArgumentException(nameof(AnimeFaceDetectCascadePath), "anime face detect cascade path is null or empty.");
		}

		// if (string.IsNullOrEmpty(TulingApiKey))
		// {
		//     throw new ArgumentNullException(nameof(TulingApiKey),
		//     "tuling api key is not provider or is empty.");
		// }
	}

}
