using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using System.IO;
using Fangzi.Bot.Extensions;

namespace Fangzi.Bot.Services
{

    public class SpeakerService: ISpeaker
    {
        readonly IAppConfig _config;

        readonly SpeechConfig _speechConfig;
        public SpeakerService(IAppConfig config)
        {
            _config = config;
            _speechConfig = SpeechConfig.FromSubscription(config.SpeechSubscription, config.SpeechRegion);
            _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz160KBitRateMonoMp3);
        }
        public async Task<AudioStream> SpeakAsync(string text, Boolean useNeural = false)
        {
            using var synthesizer = new SpeechSynthesizer(_speechConfig, null);
            var template = File.ReadAllText("./ssml.template.xml");
            var ssml = String.Format(template, text, useNeural ? "zh-CN-XiaoxiaoNeural" : "zh-CN-Yaoyao-Apollo");
            using var result = await synthesizer.SpeakSsmlAsync(ssml);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                return AudioDataStream.FromResult(result);
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    Console.WriteLine($"CANCELED: Did you update the subscription info?");
                }
            }
            return null;

        }
    }
}