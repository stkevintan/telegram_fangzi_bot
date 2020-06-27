using System;
using System.Collections.Generic;
using System.Net.Http;
namespace Music.Netease
{
    public interface IRequestProvider
    {
        Uri PublicUri { get; }

        Dictionary<string, string> Headers { get; }

        void InitCookies();

        bool Match(string path);

        HttpRequestMessage? CreateHttpRequestMessage(string path, Dictionary<string, object>? body, HttpMethod? method);
        string ResponsePipe(string text);
    }
    // public abstract class IEnhancedHttpRequestMessage : HttpRequestMessage
    // {
    //     public new HttpMethod Method { get; set; } = HttpMethod.Post;
    //     public abstract void SetContent(Dictionary<string, object> body);
    // }
}