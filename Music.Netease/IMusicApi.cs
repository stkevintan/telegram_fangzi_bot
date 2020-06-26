using System.Collections.Generic;
using System.Threading.Tasks;
using Music.Netease.Models;

namespace Music.Netease
{
    public interface IMusicApi
    {
        Task<ListResult<T>> SearchAsync<T>(string keyword, int offset = 0, bool total = true, int limit = 50) where T : BaseModel;

        Task<User> LoginAsync(string username, string password);

        Task<int> DailyTaskAsync(bool isMobile = true);

        Task<ListResult<Playlist>> UserPlaylistAsync(long uid, int offset = 0, int limit = 50);

        Task<List<T>> RecommendAsync<T>() where T : BaseModel;
    }
}