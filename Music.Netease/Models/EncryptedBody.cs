using System.Collections.Generic;

#nullable disable
namespace Music.Netease.Models
{
    public class EncryptedBody
    {
        public string @params { get; set; }
        public string encSecKey { get; set; }

        public Dictionary<string, string> AsEnumerable()
        {
            // var nvc = new List<KeyValuePair<string, string>>();
            // nvc.Add(new KeyValuePair<string, string>(nameof(@params), @params));
            // nvc.Add(new KeyValuePair<string, string>(nameof(encSecKey), encSecKey));
            // return nvc;
            return new Dictionary<string, string> {
                {nameof(@params), @params},
                {nameof(encSecKey), encSecKey}
            };
        }
    }
}