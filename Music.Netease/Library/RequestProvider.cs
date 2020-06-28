using System.Net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Music.Netease.Models;

namespace Music.Netease.Library
{
    public abstract class RequestProvider : IDisposable
    {
        protected readonly HttpClient client;

        protected readonly CookieContainer cookieJar;

        public RequestProvider(CookieContainer cookieJar)
        {
            this.cookieJar = cookieJar;
            client = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = cookieJar,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
        }

        public void Dispose()
        {
            client.Dispose();
        }
        public abstract Uri PublicUri { get; }

        public abstract Dictionary<string, string> Headers { get; }

        public virtual void InitCookies() { }

        public abstract bool Match(string path);

        protected abstract HttpRequestMessage? CreateHttpRequestMessage(string path, Dictionary<string, object>? body, HttpMethod? method);
        // string OnRequested(string text) => text;

        protected virtual void EnsureSuccessResultCode(string str)
        {
            var result = JsonConvert.DeserializeObject<ErrorResponse>(str, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
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
        public virtual async Task<string> RequestAsync(string path, Dictionary<string, object>? body, HttpMethod? method)
        {
            var response = await client.SendAsync(CreateHttpRequestMessage(path, body, method));
            response.EnsureSuccessStatusCode();
            var str = await response.Content.ReadAsStringAsync();
            EnsureSuccessResultCode(str);
            return str;
        }
    }


}