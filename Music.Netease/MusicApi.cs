using System.Reflection;
using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Music.Netease.Models;
using Music.Netease.Library;


namespace Music.Netease
{
    public class MusicApi : IMusicApi, IDisposable
    {
        CookieContainer cookieJar;

        IStorage? storage;

        List<RequestProvider> requestProviders = new List<RequestProvider>();

        public User? Me { get; private set; }

        public MusicApi(IStorage? storage = null)
        {
            this.storage = storage;
            var session = storage?.LoadSession();
            if (session != null)
            {
                Me = session.User;
                cookieJar = session.CookieJar;
            }
            else
            {
                cookieJar = new CookieContainer();
            }
            initRequestProviders();

            checkSession();
        }
        void initRequestProviders()
        {
            var rType = typeof(RequestProvider);
            var assembly = Assembly.GetAssembly(rType)!;
            var list = from t in assembly.GetExportedTypes() where rType.IsAssignableFrom(t) && !t.IsAbstract select t;
            foreach (var t in list)
            {
                requestProviders.Add((RequestProvider)Activator.CreateInstance(t, cookieJar)!);
            }
        }
        void checkSession()
        {
            foreach (var rp in requestProviders)
            {
                var cookies = cookieJar.GetCookies(rp.PublicUri);
                rp.InitCookies();
                // if cookie is expired, clear the login status.
                var count = (from c in cookies where c.Expired select c).Count();
                if (count > 0)
                {
                    cookies.Clear();
                    Me = null;
                    storage?.ClearSession();
                    return;
                }
            }
        }

        public void Dispose()
        {
            if (Me != null)
            {
                storage?.SaveSession(new Session(Me, cookieJar));
            }
            else
            {
                storage?.ClearSession();
            }
            requestProviders.ForEach(r => r.Dispose());
        }

        ResponseResolver Request(
           string path,
           Dictionary<string, object>? body = null,
           HttpMethod? method = null)
        {
            var provider = (from r in requestProviders where r.Match(path) select r).Single();
            return new ResponseResolver(provider.RequestAsync(path, body, method));
        }

        #region API
        public async Task<User> LoginAsync(string username, string password)
        {
            var url = "/weapi/login/cellphone";
            var body = new Dictionary<string, object>
            {
                {"phone", username},
                {"password", Encrypt.Md5(password)},
                {"rememberLogin", "true"}
            };
            var res = await Request(url, body).Into(new { Profile = default(User) });
            return Me = Utility.AssertNotNull(res?.Profile);
        }

        public async Task<ListResult<T>> SearchAsync<T>(string keyword, int offset = 0, bool total = true, int limit = 50) where T : BaseModel
        {
            var url = "/weapi/search/get";
            var type = typeof(T);
            EntityInfoAttribute? entityInfo = (EntityInfoAttribute?)Attribute.GetCustomAttribute(type, typeof(EntityInfoAttribute));
            if (entityInfo == null)
            {
                throw new NotSupportedException($"Cannot search type {type.Name}, Only {nameof(Song)}, {nameof(Album)}, {nameof(Artist)} {nameof(User)} and {nameof(Playlist)} are supported.");
            }
            var typeName = entityInfo.Name ?? type.Name;

            var body = new Dictionary<string, object> {
                {"s", keyword},
                {"type", entityInfo.Type},
                {"offset", offset},
                {"total", total ? "true":"false"},
                {"limit", limit}
            };
            var res = await Request(url, body).Into(
                new { Result = default(ListResult<T>) },
                new JsonSerializerSettings
                {
                    ContractResolver = new DynamicContractResolver
                    {
                        Renames = new Dictionary<string, string> {
                            {"Items",  typeName + "s"},
                            {"Count", typeName + "Count"}
                        }
                    }
                });
            return Utility.AssertNotNull(res?.Result);
        }

        public async Task<int> DailyTaskAsync(bool isMobile = true)
        {
            var path = "/weapi/point/dailyTask";
            var body = new Dictionary<string, object> { { "type", isMobile ? 0 : 1 } };
            var ret = await Request(path, body).Into(new { Point = default(int) });
            //"{\"point\":4,\"code\":200}"
            return Utility.AssertNotNull(ret).Point;
        }

        public async Task<List<Playlist>> UserPlaylistAsync(long uid, int offset = 0, int limit = 50)
        {
            var path = "/weapi/user/playlist";
            var body = new Dictionary<string, object> {
                {"uid", uid},
                {"offset", offset},
                {"limit", limit},
            };
            var ret = await Request(path, body).Into(new { Playlist = default(List<Playlist>) });
            return Utility.AssertNotNull(ret?.Playlist);
        }

        public async Task<List<T>> RecommendAsync<T>() where T : BaseModel
        {
            var type = typeof(T);
            var path = "/weapi/v1/discovery/recommend";
            if (type.Name == "Playlist")
            {
                path += "/resource";
            }
            else if (type.Name == "Song")
            {
                path += "/songs";
            }
            else
            {
                throw new NotSupportedException($"Recommend {type.Name} is not supported");
            }
            var ret = await Request(path).Into(new { Recommend = default(List<T>) });
            return Utility.AssertNotNull(ret?.Recommend);
        }

        public async Task<List<Song>> PersonalFmAsync()
        {
            var path = "/weapi/v1/radio/get";
            var ret = await Request(path).Into(new { Data = default(List<Song>) });
            return Utility.AssertNotNull(ret?.Data);
        }

        /// <summary>
        ///  Get Song's playback url
        /// </summary>
        public async Task<SongUrl> SongUrlAsync(long Id, int br = 999000)
        {
            var path = "/eapi/song/enhance/player/url";
            var body = new Dictionary<string, object> {
                {"ids", new long[] {Id}},
                {"br", br}
            };
            var ret = await Request(path, body).Into(new { Data = default(List<SongUrl>) });
            return Utility.AssertNotNull(ret?.Data?.FirstOrDefault());
        }

        public async Task<List<SongUrl>> SongUrlAsync(List<long> IdList, int br = 999000)
        {
            var path = "/eapi/song/enhance/player/url";
            var body = new Dictionary<string, object> {
                {"ids", IdList},
                {"br", br}
            };
            var ret = await Request(path, body).Into(new { Data = default(List<SongUrl>) });
            return Utility.AssertNotNull(ret?.Data);
        }

        public async Task<SongLyric> SongLyricAsync(long Id)
        {
            var path = "/eapi/song/lyric";
            var body = new Dictionary<string, object> {
                {"os", "pc"},
                {"id", Id},
                {"lv", -1},
                {"kv", -1},
                {"tv", -1},
            };
            var ret = await Request(path, body).Into<SongLyric>();
            return Utility.AssertNotNull(ret);
        }

        public async Task<SongDetail> SongDetail(long Id)
        {
            var path = "/weapi/v3/song/detail";
            var body = new Dictionary<string, object> {
                {"c", $"[{{\"id\": {Id}}}]"},
                {"ids", $"[{Id}]"}
            };
            var ret = await Request(path, body).Into(new { Songs = default(List<SongDetail>) });
            return Utility.AssertNotNull(ret?.Songs?.FirstOrDefault());
        }

        public async Task<List<SongDetail>> SongDetail(List<long> Ids)
        {
            var path = "/weapi/v3/song/detail";
            var body = new Dictionary<string, object> {
                {"c", $"[{String.Join(',', Ids.Select(id => $"{{\"id\": {id}}}"))}]"},
                {"ids", $"[{String.Join(',', Ids)}]"}
            };
            var ret = await Request(path, body).Into(new { Songs = default(List<SongDetail>) });
            return Utility.AssertNotNull(ret?.Songs);
        }

        public async Task<Album> AlbumDetail(long Id)
        {
            var path = $"/weapi/v1/album/{Id}";
            var ret = await Request(path).Into(new { Album = default(Album), Songs = default(List<SongDetail>) });
            if (ret?.Album != null)
            {
                ret.Album.Songs = ret.Songs ?? new List<SongDetail>();
                return ret.Album;
            }
            throw new NullReferenceException();
        }

        public async Task<List<Album>> ArtistAlbum(long Id, int limit = 30, int offset = 0)
        {
            var path = $"/weapi/artist/albums/{Id}";
            var body = new Dictionary<string, object>
            {
                ["limit"] = limit,
                ["offset"] = offset,
                ["total"] = true
            };
            var ret = await Request(path, body).Into(new { HotAlbums = default(List<Album>) });
            return Utility.AssertNotNull(ret?.HotAlbums);
        }

        public async Task<string> ArtistDesc(long Id)
        {
            var path = $"/weapi/artist/introduction";
            var body = new Dictionary<string, object> {
                {"id", Id}
            };
            var ret = await Request(path, body).Into();
            return Utility.AssertNotNull(ret);
        }
        #endregion API
    }
}