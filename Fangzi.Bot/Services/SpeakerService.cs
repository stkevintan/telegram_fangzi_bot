using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Fangzi.Bot.Extensions;
using Fangzi.Bot.Interfaces;

namespace Fangzi.Bot.Services
{

    public class SpeakerService : ISpeaker
    {
        static string SSML_TEMPLATE = @"
        <speak version=""1.0"" 
            xmlns=""https://www.w3.org/2001/10/synthesis""
            xmlns:mstts=""https://www.w3.org/2001/mstts"" xml:lang=""zh-CN"">
            <voice name=""{1}"">
                <mstts:express-as style=""assistant"">
                    <prosody rate=""+20.00%"">
                        {0}
                    </prosody>
                </mstts:express-as>
            </voice>
        </speak>";
        readonly BotConfiguration _config;

        readonly SpeechConfig _speechConfig;
        public SpeakerService(BotConfiguration config)
        {
            _config = config;
            _speechConfig = SpeechConfig.FromSubscription(config.SpeechSubscription, config.SpeechRegion);
            _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz160KBitRateMonoMp3);
        }
        public async Task<AudioStream?> SpeakAsync(string text, string voiceKind = "zh-CN-XiaoxiaoNeural")
        {
            using var synthesizer = new SpeechSynthesizer(_speechConfig, null);
            // https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/language-support#prebuilt-neural-voices
            var ssml = String.Format(SSML_TEMPLATE, text, voiceKind);
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