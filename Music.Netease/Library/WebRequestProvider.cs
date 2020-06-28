using System.Net;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Net.Http;
namespace Music.Netease.Library
{
    public class WebRequestProvider : RequestProvider
    {
        static readonly Dictionary<string, string> headers = new Dictionary<string, string>
        {
                {"Accept", "*/*"},
                {"Accept-Language", "zh-CN,zh;q=0.8,gl;q=0.6,zh-TW;q=0.4"},
                {"Connection", "keep-alive"},
                {"Host", "music.163.com"},
                {"Referer", @"http://music.163.com"},
                {"User-Agent", @"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36"}
        };

        static readonly Uri publicUri = new Uri("http://music.163.com");

        public override Uri PublicUri => publicUri;

        public override Dictionary<string, string> Headers => headers;


        public WebRequestProvider(CookieContainer cookieJar) : base(cookieJar) { }
        public override bool Match(string path)
        {
            return path.StartsWith("/weapi/");
        }
        protected override HttpRequestMessage? CreateHttpRequestMessage(string path, Dictionary<string, object>? body, HttpMethod? method)
        {
            if (!Match(path)) return null;
            method = method ?? HttpMethod.Post;
            var req = new HttpRequestMessage
            {
                RequestUri = new Uri(publicUri, path),
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
            body["csrf_token"] = (from c in cookies where c.Name == "__csrf" select c.Value).FirstOrDefault();
            req.Content = new FormUrlEncodedContent(Encrypt.EncryptWebRequest(body));
            return req;
        }
    }
}