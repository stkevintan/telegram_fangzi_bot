using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Music.Netease.Test
{
    public static class Configuration
    {
        public static readonly string Username;

        public static readonly string Password;

        public static readonly string Nickname;

        public static readonly string Root = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        static Configuration()
        {
            //"/home/kevin/Projects/Fangzi.Telegram.Bot/Music.Netease.Test/bin/Debug/netcoreapp3.1"
            var conf = new ConfigurationBuilder()
            .SetBasePath(Root)
            .AddJsonFile("appsettings.json")
            .Build();

            Username = conf["username"];
            Password = conf["password"];
            Nickname = conf["nickname"];
            if (String.IsNullOrEmpty(Username) || String.IsNullOrEmpty(Password) || String.IsNullOrEmpty(Nickname))
            {
                throw new ArgumentNullException("Cannot find username, password or nickname section in appsettings.json");
            }
        }
    }
}