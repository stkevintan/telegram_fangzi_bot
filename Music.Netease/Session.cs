using System.Net;
using Music.Netease.Models;

namespace Music.Netease
{
    public class Session
    {
        public Session(User user, CookieContainer cookieJar)
        {
            User = user;
            CookieJar = cookieJar;
        }
        public User User { get; set; }
        public CookieContainer CookieJar { get; set; }
    }

}