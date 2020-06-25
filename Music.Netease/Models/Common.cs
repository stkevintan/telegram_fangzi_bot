using System.Collections.Generic;
namespace Music.Netease.Models
{
    public class ListResult<T>
    {
        public List<T> Items { get; set; }
        public int Count { get; set; }
    }
    public interface BaseModel
    {
        long Id { get; set; }
        string Name { get; set; }
    }
}