using System.Net;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Net.Http;
namespace Music.Netease.Library
{
    public class PCRequestProvider : IRequestProvider
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
        Encrypt enc = new Encrypt();

        public Uri PublicUri => publicUri;

        public Dictionary<string, string> Headers => headers;


        CookieContainer cookieJar;
        public PCRequestProvider(CookieContainer cookieJar)
        {
            this.cookieJar = cookieJar;
        }

        public void InitCookies()
        {
            var cookies = new CookieCollection();
            cookies.Add(new Cookie("os", "pc"));
            cookies.Add(new Cookie("osver", "Microsoft-Windows-10-Enterprise-Edition-build-19041-64bit"));
            cookies.Add(new Cookie("appver", "2.7.1.198242"));
            cookies.Add(new Cookie("mode", "20NYS3EP0J"));
            cookies.Add(new Cookie("channel", "netease"));
            cookies.Add(new Cookie("deviceId", "89D32E91D9266D763069C70FE495CFAAA6A7B06FF875A4A3DD42"));
            cookieJar.Add(publicUri, cookies);
        }

        public bool Match(string path)
        {
            return path.StartsWith("/eapi/");
        }

        /* SampleResult
        /api/song/lyric-36cd479b6b5-{"os":"pc","id":"4875310","lv":"-1","kv":"-1","tv":"-1","e_r":true,"header":"{\"os\":\"pc\",\"appver\":\"2.7.1.198242\",\"deviceId\":\"89D32E91D9266D763069C70FE495CFAAA6A7B06FF875A4A3DD42\",\"requestId\":\"39489139\",\"clientSign\":\"80:32:53:62:EA:62@@@354344325F453432395F393134335F323832382E@@@@@@09f160ca-67f8-45fa-bf59-1c7d5f1d0f1cafdcbed7a76f98550ab960d515230587\",\"osver\":\"Microsoft-Windows-10-Enterprise-Edition-build-19041-64bit\",\"MUSIC_U\":\"a7b1b33884539e708dbde110717061d192583b5bce3eb09f6bc857199e66f65733a649814e309366\"}"}-36cd479b6b5-4aa30fcdc6c1fb2c6eedccfc90114f0d
        */
        public HttpRequestMessage? CreateHttpRequestMessage(string path, Dictionary<string, object>? body, HttpMethod? method)
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
            body["header"] = new Dictionary<string, object> {
                        {"osver", cookies["osver"] },
                        {"deviceId", cookies["deviceId"]},
                        {"appver", cookies["appver"]},
                        {"os", cookies["os"]},
                        {"requestId", Guid.NewGuid().ToString("N") },
                        {"MUSIC_A", cookies["MUSIC_A"]},
                        {"MUSIC_U", cookies["MUSIC_U"]}
                        // {"clientSign"}
                    };
            req.Content = new FormUrlEncodedContent(enc.EncryptPCRequest(req.RequestUri.AbsoluteUri, body));
            return req;
        }

        public string ResponsePipe(string text)
        {
            return enc.DecryptPCResponse(text);
        }
    }
}