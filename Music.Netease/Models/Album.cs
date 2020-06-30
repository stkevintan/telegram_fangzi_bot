using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Music.Netease.Library;

/*
{
  "songs": [],
  "paid": false,
  "onSale": false,
  "mark": 0,
  "description": null,
  "picUrl": "http://p4.music.126.net/6zzWtkMnCAD4yxk_r4CUJA==/2528876746537476.jpg",
  "pic": 2528876746537476,
  "status": 0,
  "alias": [],
  "artists": [
    {
      "img1v1Id": 18686200114669624,
      "topicPerson": 0,
      "picUrl": "",
      "alias": [],
      "picId": 0,
      "followed": false,
      "albumSize": 0,
      "briefDesc": "",
      "trans": "",
      "musicSize": 0,
      "img1v1Url": "http://p3.music.126.net/VnZiScyynLG7atLIZ2YPkw==/18686200114669622.jpg",
      "name": "新垣結衣",
      "id": 15988,
      "img1v1Id_str": "18686200114669622"
    }
  ],
  "copyrightId": 0,
  "picId": 2528876746537476,
  "artist": {
    "img1v1Id": 18686200114669624,
    "topicPerson": 0,
    "picUrl": "http://p3.music.126.net/A44WBdM_r_pqMnt2U3mDbg==/3440371895759361.jpg",
    "alias": [
      "Aragaki Yui"
    ],
    "picId": 3440371895759361,
    "followed": false,
    "albumSize": 8,
    "briefDesc": "",
    "trans": "",
    "musicSize": 69,
    "img1v1Url": "http://p3.music.126.net/VnZiScyynLG7atLIZ2YPkw==/18686200114669622.jpg",
    "name": "新垣結衣",
    "id": 15988,
    "img1v1Id_str": "18686200114669622"
  },
  "blurPicUrl": "http://p4.music.126.net/6zzWtkMnCAD4yxk_r4CUJA==/2528876746537476.jpg",
  "companyId": 0,
  "briefDesc": null,
  "commentThreadId": "R_AL_3_3086101",
  "publishTime": 1257436800007,
  "company": "华纳音乐",
  "subType": "录音室版",
  "tags": "",
  "name": "小さな恋のうた",
  "id": 3086101,
  "type": "EP/Single",
  "size": 1,
  "info": {
    "commentThread": {
      "id": "R_AL_3_3086101",
      "resourceInfo": {
        "id": 3086101,
        "userId": -1,
        "name": "小さな恋のうた",
        "imgUrl": "https://p3.music.126.net/6zzWtkMnCAD4yxk_r4CUJA==/2528876746537476.jpg",
        "creator": null,
        "encodedId": null,
        "subTitle": null,
        "webUrl": null
      },
      "resourceType": 3,
      "commentCount": 38,
      "likedCount": 0,
      "shareCount": 50,
      "hotCount": 3,
      "latestLikedUsers": null,
      "resourceOwnerId": -1,
      "resourceTitle": "小さな恋のうた",
      "resourceId": 3086101
    },
    "latestLikedUsers": null,
    "liked": false,
    "comments": null,
    "resourceType": 3,
    "resourceId": 3086101,
    "commentCount": 38,
    "likedCount": 0,
    "shareCount": 50,
    "threadId": "R_AL_3_3086101"
  }
}
*/
#nullable disable
namespace Music.Netease.Models
{
    [EntityInfo(10)]
    public class Album : BaseModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<string> Alias { get; set; }

        public List<Artist> Artists { get; set; }

        [JsonConverter(typeof(DateConverter))]
        public DateTime PublishTime { get; set; }

        public List<SongDetail> Songs { get; set; }

        public int Size { get; set; }

        public int Status { get; set; }

        public long PicId { get; set; }

        public string PicUrl { get; set; }

        public string Type { get; set; }

        public List<string> Tns { get; set; }

        [JsonExtensionData]
        private Dictionary<string, JToken> raw { get; set; } = new Dictionary<string, JToken>();

    }
}