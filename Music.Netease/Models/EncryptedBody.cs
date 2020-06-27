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
            return new Dictionary<string, string> {
                {nameof(@params), @params},
                {nameof(encSecKey), encSecKey}
            };
        }
    }
}