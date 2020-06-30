using Music.Netease.Library;
using System.Collections.Generic;

#nullable disable
namespace Music.Netease.Models
{
    [EntityInfo(1)]
    public class Song : BaseModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<string> Alias { get; set; }

        public long Mvid { get; set; }

        public List<Artist> Artists { get; set; }

        public Album Album { get; set; }

        public int Duration { get; set; }

        public int Status { get; set; }
    }
}