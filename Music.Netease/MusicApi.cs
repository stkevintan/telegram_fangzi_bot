using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Music.Netease.Models;
using Music.Netease.Library;

namespace Music.Netease
{
    public class MusicApi : IMusicApi, IDisposable
    {
        const string PUBLIC_PATH = "http://music.163.com";
        private static readonly Encrypt enc = new Encrypt();
        private static readonly Dictionary<string, string> headers = new Dictionary<string, string>
        {
                {"Accept", "*/*"},
                {"Accept-Language", "zh-CN,zh;q=0.8,gl;q=0.6,zh-TW;q=0.4"},
                {"Connection", "keep-alive"},
                {"Host", "music.163.com"},
                {"Referer", @"http://music.163.com"},
                {"User-Agent", @"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36"}
        };
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
        CookieContainer cookieContainer;
        readonly HttpClient client;

        public MusicApi()
        {
            cookieContainer = new CookieContainer();
            // cookieContainer.Add(new Cookie("os", "pc", PUBLIC_PATH, PUBLIC_PATH));
            client = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = cookieContainer,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
        }

        public void Dispose()
        {
            client.Dispose();
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

        public async Task<ListResult<Playlist>> UserPlaylistAsync(long uid, int offset = 0, int limit = 50)
        {
            var path = "/weapi/user/playlist";
            var body = new Dictionary<string, object> {
                {"uid", uid},
                {"offset", offset},
                {"limit", limit},
            };
            return AssertNotNull(await RequestAsync<ListResult<Playlist>>(path, body));
        }

        // Recommend playlist
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
            return AssertNotNull(res?.Profile);
        }


        async Task<string> RawRequestAsync(string path, Dictionary<string, object>? body = null, HttpMethod? method = null)
        {
            var endpoint = new Uri($"{PUBLIC_PATH}{path}");
            if (method == null)
            {
                method = HttpMethod.Post;
            }
            var cookies = cookieContainer.GetCookies(endpoint);
            var csrfToken = (from c in cookies where c.Name == "__csrf" select c.Value).FirstOrDefault();
            if (method == HttpMethod.Post && !String.IsNullOrEmpty(csrfToken))
            {
                if (body == null)
                {
                    body = new Dictionary<string, object>();
                }
                body["csrf_token"] = csrfToken;
            }

            var req = new HttpRequestMessage
            {
                Method = method,
                RequestUri = endpoint,
                Content = body == null ? null : new FormUrlEncodedContent(enc.EncrptedRequest(body).AsEnumerable())
            };
            headers.ToList().ForEach(pair => req.Headers.Add(pair.Key, pair.Value));
            HttpResponseMessage response = await client.SendAsync(req);
            response.EnsureSuccessStatusCode();
            var str = await response.Content.ReadAsStringAsync();
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