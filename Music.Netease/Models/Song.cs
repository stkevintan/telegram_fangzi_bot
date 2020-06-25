using Music.Netease.Library;
using System.Collections.Generic;
namespace Music.Netease.Models
{
    [EntityInfo(1)]
    public class Song : BaseModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<Artist> Artist { get; set; }
        public Album Album { get; set; }
        public int Duration { get; set; }
        public int Status { get; set; }
    }
}