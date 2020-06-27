using Music.Netease.Library;


#nullable disable
namespace Music.Netease.Models
{
    [EntityInfo(1000)]
    public class Playlist : BaseModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int BookCount { get; set; }

        public string CoverImgUrl { get; set; }

        public User Creator { get; set; }

        public string Description { get; set; }

        public bool HighQuality { get; set; }

        public int PlayCount { get; set; }

        public bool Subscribed { get; set; }

        public int TrackCount { get; set; }

        public long UserId { get; set; }
    }
}