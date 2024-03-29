using System.Threading.Tasks;
using Fangzi.Bot.Extensions;

namespace Fangzi.Bot.Interfaces
{
    public interface ISpeaker
    {
        public Task<AudioStream?> SpeakAsync(string text, string voiceKind = "zh-CN-XiaoxiaoNeural");
    }
}