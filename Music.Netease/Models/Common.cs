using System;
using System.Collections.Generic;

#nullable disable
namespace Music.Netease.Models
{
    public class ListResult<T>
    {
        public List<T> Items { get; set; }
        public int Count { get; set; }
    }

    public class Result<T, E> where E: Exception {
        
    }
    public interface BaseModel
    {
        long Id { get; set; }
        string Name { get; set; }
    }

    public class BaseResponse
    {
        public int Code { get; set; } = -1;
    }

    public class ErrorResponse : BaseResponse
    {
        public string Msg { get; set; }
        public string Message { get; set; }
    }
}