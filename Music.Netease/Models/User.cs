using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Music.Netease.Library;
namespace Music.Netease.Models
{
    [EntityInfo(1002, Name = "Userprofile")]
    public class User : BaseModel
    {
        [JsonProperty(PropertyName = "userId")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "nickname")]
        public string Name { get; set; }

        public int UserType { get; set; }

        public int AuthStatus { get; set; }

        public string AvatarImgUrl { get; set; }

        public long Birthday { get; set; }

        public string City { get; set; }

        public bool Followed { get; set; }
        public int Gender { get; set; }
        public string Province { get; set; }
        public int vipType { get; set; }
    }
}