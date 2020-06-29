using System.Net;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace Music.Netease.Library
{
    public class PCRequestProvider : RequestProvider
    {
        static readonly Dictionary<string, string> headers = new Dictionary<string, string>
        {
                {"Accept", "*/*"},
                {"Accept-Language", "zh-CN,zh;q=0.8,gl;q=0.6,zh-TW;q=0.4"},
                {"Connection", "keep-alive"},
                {"Host", "interface.music.163.com"},
                {"User-Agent", @"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.157 NeteaseMusicDesktop/2.7.1.198242 Safari/537.36"}
        };

        static readonly Uri publicUri = new Uri("https://interface.music.163.com");

        public override Uri PublicUri => publicUri;

        public override Dictionary<string, string> Headers => headers;

        public PCRequestProvider(CookieContainer cookieJar) : base(cookieJar) { }

        Cookie createCookie(string key, string value)
        {
            return new Cookie(key, value, "/", ".music.163.com");
        }

        string? getCookieValue(string key)
        {
            var cookies = cookieJar.GetCookies(PublicUri);
            return cookies.Where(c => c.Name == key).Select(c => c.Value).FirstOrDefault();
        }
        public override void InitCookies()
        {
            cookieJar.Add(createCookie("os", "pc"));
            cookieJar.Add(createCookie("osver", "Microsoft-Windows-10-Enterprise-Edition-build-19041-64bit"));
            cookieJar.Add(createCookie("appver", "2.7.1.198242"));
            cookieJar.Add(createCookie("mode", "20NYS3EP0J"));
            cookieJar.Add(createCookie("channel", "netease"));
        }

        public override bool Match(string path)
        {
            return path.StartsWith("/eapi/");
        }


        protected override HttpRequestMessage? CreateHttpRequestMessage(string path, Dictionary<string, object>? body, HttpMethod? method)
        {
            if (!Match(path)) return null;
            method = method ?? HttpMethod.Post;
            var req = new HttpRequestMessage
            {
                RequestUri = new Uri(PublicUri, path),
                Method = method,
            };
            foreach (var pair in headers)
            {
                req.Headers.Add(pair.Key, pair.Value);
            }
            if (method != HttpMethod.Post)
            {
                return req;
            }
            body = body ?? new Dictionary<string, object>();
            var cookies = cookieJar.GetCookies(publicUri);
            // cookies.
            var header = new Dictionary<string, object?>
            {
                ["osver"] = getCookieValue("osver"),
                ["deviceId"] = getCookieValue("deviceId"),
                ["appver"] = getCookieValue("appver"),
                ["os"] = getCookieValue("os"),
                ["requestId"] = Guid.NewGuid().ToString("N"),
                ["MUSIC_A"] = getCookieValue("MUSIC_A"),
                ["MUSIC_U"] = getCookieValue("MUSIC_U"),
                ["clientSign"] = getCookieValue("clientSign")
            };
            body["header"] = Utility.RemoveNullEntries(header);
            body["e_r"] = true;
            req.Content = new FormUrlEncodedContent(Encrypt.EncryptPCRequest(path, body));
            return req;
        }
        public override async Task<string> RequestAsync(string path, Dictionary<string, object>? body, HttpMethod? method)
        {
            var response = await client.SendAsync(CreateHttpRequestMessage(path, body, method));
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var str = Encrypt.DecryptPCResponse(bytes);
            EnsureSuccessResultCode(str);
            return str;
        }
    }
}