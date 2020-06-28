using System.Text;
using System.Security.Cryptography;
using System;
namespace Music.Netease.Library
{
    public static class Utility
    {
        public static T AssertNotNull<T>(T? obj) where T : class
        {
            if (obj == null)
            {
                throw new NullReferenceException();
            }
            return obj;
        }

    }
}