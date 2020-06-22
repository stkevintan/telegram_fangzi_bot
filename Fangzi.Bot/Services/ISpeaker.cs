using System.Threading.Tasks;
using Fangzi.Bot.Extensions;

namespace Fangzi.Bot
{
    public interface ISpeaker
    {
        public Task<AudioStream> SpeakAsync(string text, bool useNeural);
    }
}