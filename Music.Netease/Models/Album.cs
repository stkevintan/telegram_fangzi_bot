using Music.Netease.Library;

namespace Music.Netease.Models
{
    [EntityInfo(10)]
    public class Album : BaseModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Artist Artist { get; set; }

        public long PublishTime { get; set; }

        public int Size { get; set; }

        public int Status { get; set; }

        public long PicId { get; set; }
    }
}