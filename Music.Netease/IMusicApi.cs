using System;
using System.Threading.Tasks;
using Music.Netease.Models;

namespace Music.Netease
{
    public interface IMusicApi
    {
        Task<ListResult<T>> Search<T>(string keyword, int offset = 0, bool total = true, int limit = 50) where T : BaseModel;
    }
}