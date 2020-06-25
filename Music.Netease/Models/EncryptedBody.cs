using System.Collections.Generic;
namespace Music.Netease.Models
{
    public class EncryptedBody
    {
        public string @params { get; set; }
        public string encSecKey { get; set; }

        public List<KeyValuePair<string, string>> toList()
        {
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>(nameof(@params), @params));
            nvc.Add(new KeyValuePair<string, string>(nameof(encSecKey), encSecKey));
            return nvc;
        }
    }
}