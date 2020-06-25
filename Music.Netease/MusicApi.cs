using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Music.Netease.Models;
using Music.Netease.Library;

namespace Music.Netease
{
    public class MusicApi : IMusicApi
    {
        const string baseURL = "http://music.163.com";
        static Encrypt enc = new Encrypt();
        static readonly HttpClient client = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });
        static readonly Dictionary<string, string> Headers = new Dictionary<string, string>
        {
                {"Accept", "*/*"},
                {"Accept-Language", "zh-CN,zh;q=0.8,gl;q=0.6,zh-TW;q=0.4"},
                {"Connection", "keep-alive"},
                {"Host", "music.163.com"},
                {"Referer", @"http://music.163.com"},
                {"User-Agent", @"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36"}
        };

        public async Task<ListResult<T>> Search<T>(string keyword, int offset = 0, bool total = true, int limit = 50) where T : BaseModel
        {
            var url = "/weapi/search/get";
            var type = typeof(T);
            EntityInfoAttribute entityInfo = (EntityInfoAttribute)Attribute.GetCustomAttribute(type, typeof(EntityInfoAttribute));
            if (entityInfo == null)
            {
                throw new NotSupportedException($"Cannot search type {type.Name}");
            }
            var typeName = entityInfo.Name ?? type.Name;

            return await Request<ListResult<T>>(HttpMethod.Post, url, new
            {
                s = keyword,
                type = entityInfo.Type,
                offset = offset,
                total = total ? "true" : "false",
                limit = limit
            },
            new Dictionary<string, string>
            {
                {"Items", typeName + "s"},
                {"Count", typeName + "Count"}
            });
        }

        public async Task<T> Request<T>(HttpMethod method, string path, object data = null, Dictionary<string, string> renames = null)
        {
            var endpoint = new Uri($"{baseURL}{path}");
            var content = enc.EncrptedRequest(data);
            var req = new HttpRequestMessage
            {
                Method = method,
                RequestUri = endpoint,
                Content = data == null ? null : new FormUrlEncodedContent(content.toList())
            };
            Headers.ToList().ForEach(pair => req.Headers.Add(pair.Key, pair.Value));
            var response = await client.SendAsync(req);
            response.EnsureSuccessStatusCode();
            var str = await response.Content.ReadAsStringAsync();
            var ret = JsonConvert.DeserializeAnonymousType(str, new { Result = default(T) }, new JsonSerializerSettings
            {
                ContractResolver = new DynamicContractResolver
                {
                    Renames = renames
                }
            });
            return ret.Result;
        }
    }
}