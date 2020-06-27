using System.Reflection;
using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Music.Netease.Models;
using Music.Netease.Library;


namespace Music.Netease
{
    public class MusicApi : IMusicApi, IDisposable
    {
        static readonly Encrypt enc = new Encrypt();
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false
                }
            }
        };
        CookieContainer cookieJar;

        IStorage? storage;

        List<IRequestProvider> RequestProviders = new List<IRequestProvider>();

        readonly HttpClient client;

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
            client = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = cookieJar,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
        }
        void initRequestProviders()
        {
            //   RequestProviders.Add(new PCRequestProvider(cookieJar));
            // RequestProviders.Add(new WebRequestProvider(cookieJar));
            var IType = typeof(IRequestProvider);
            var assembly = Assembly.GetAssembly(IType)!;
            var list = (from t in assembly.GetExportedTypes() where IType.IsAssignableFrom(t) && !t.IsInterface select t);
            foreach (var t in list)
            {
                RequestProviders.Add((IRequestProvider)Activator.CreateInstance(t, cookieJar)!);
            }
        }
        void checkSession()
        {
            foreach (var rp in RequestProviders)
            {
                var cookies = cookieJar.GetCookies(rp.PublicUri);
                if (cookies.Count() == 0) { rp.InitCookies(); }
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
            client.Dispose();
        }

        #region API
        public async Task<User> LoginAsync(string username, string password)
        {
            var url = "/weapi/login/cellphone";
            var body = new Dictionary<string, object>
            {
                {"phone", username},
                {"password", enc.Md5(password)},
                {"rememberLogin", "true"}
            };
            var res = await RequestAsync(new { Profile = default(User) }, url, body);
            return Me = AssertNotNull(res?.Profile);
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
            var res = await RequestAsync(
                method: HttpMethod.Post,
                typeObject: new { Result = default(ListResult<T>) },
                path: url,
                body: body,
                settings: new JsonSerializerSettings
                {
                    ContractResolver = new DynamicContractResolver
                    {
                        Renames = new Dictionary<string, string> {
                            {"Items",  typeName + "s"},
                            {"Count", typeName + "Count"}
                        }
                    }
                });
            return AssertNotNull(res?.Result);
        }

        public async Task<int> DailyTaskAsync(bool isMobile = true)
        {
            var path = "/weapi/point/dailyTask";
            var body = new Dictionary<string, object> { { "type", isMobile ? 0 : 1 } };
            var ret = await RequestAsync(new { Point = default(int) }, path, body);
            //"{\"point\":4,\"code\":200}"
            return AssertNotNull(ret).Point;
        }

        public async Task<List<Playlist>> UserPlaylistAsync(long uid, int offset = 0, int limit = 50)
        {
            var path = "/weapi/user/playlist";
            var body = new Dictionary<string, object> {
                {"uid", uid},
                {"offset", offset},
                {"limit", limit},
            };
            var ret = await RequestAsync(new { Playlist = default(List<Playlist>) }, path, body);
            return AssertNotNull(ret?.Playlist);
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
            var ret = await RequestAsync(new { Recommend = default(List<T>) }, path);
            return AssertNotNull(ret?.Recommend);
        }

        public async Task<List<Song>> PersonalFmAsync()
        {
            var path = "/weapi/v1/radio/get";
            var ret = await RequestAsync(new { Data = default(List<Song>) }, path);
            return AssertNotNull(ret?.Data);
        }

        /// <summary>
        ///  Get Song's playback url
        /// </summary>
        public async Task SongUrlAsync(long Id, int br = 999000)
        {
            var path = "/weapi/song/enhance/player/url";
            var body = new Dictionary<string, object> {
                {"ids", new long[] {Id}},
                {"br", br}
            };
            var ret = await RawRequestAsync(path, body);
        }
        #endregion API

        async Task<string> RawRequestAsync(string path, Dictionary<string, object>? body = null, HttpMethod? method = null)
        {
            var reqProvider = (
                from rp in RequestProviders
                where rp.Match(path)
                select rp
            ).Single();
            var resMessage = reqProvider.CreateHttpRequestMessage(path, body, method);
            HttpResponseMessage response = await client.SendAsync(resMessage);
            response.EnsureSuccessStatusCode();
            var str = await response.Content.ReadAsStringAsync();
            str = reqProvider.ResponsePipe(str);
            EnsureSuccessResultCode(str);
            return str;
        }

        T AssertNotNull<T>(T? obj) where T : class
        {
            if (obj == null)
            {
                throw new NullReferenceException();
            }
            return obj;
        }
        void EnsureSuccessResultCode(string str)
        {
            var result = JsonConvert.DeserializeObject<ErrorResponse>(str, settings);
            if (result == null)
            {
                throw new HttpRequestException($"Response is empty");
            }
            if (result.Code != 200)
            {
                throw new HttpRequestException($"Request failed with {result.Code}, Detail: {result.Message ?? result.Msg}")
                {
                    Data = {
                        {"Code", result.Code},
                        {"Message", result.Message ?? result.Msg}
                    }
                };
            }
        }
        async Task<T?> RequestAsync<T>(
            string path,
            Dictionary<string, object>? body = null,
            HttpMethod? method = null,
            JsonSerializerSettings? settings = null) where T : class
        {
            var str = await RawRequestAsync(path, body, method);
            return JsonConvert.DeserializeObject<T>(str, settings ?? MusicApi.settings);
        }
        async Task<T?> RequestAsync<T>(
            T typeObject,
            string path,
            Dictionary<string, object>? body = null,
            HttpMethod? method = null,
            JsonSerializerSettings? settings = null) where T : class
        {
            var str = await RawRequestAsync(path, body, method);
            return JsonConvert.DeserializeAnonymousType(str, typeObject, settings ?? MusicApi.settings);
        }
    }
}