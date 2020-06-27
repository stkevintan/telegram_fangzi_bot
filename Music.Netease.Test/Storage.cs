using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;
using Music.Netease.Models;

namespace Music.Netease.Test
{

#nullable enable
    public class Storage : IStorage
    {
        static readonly string TempFile = Path.Join(Path.GetTempPath(), "music_netease_cookie.dat");
        public Session? LoadSession()
        {
            try
            {
                using var stream = File.OpenRead(TempFile);
                BinaryFormatter formatter = new BinaryFormatter();
                return (Session2)formatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException) return null;
                ClearSession();
                throw e;
            }
        }

        public void SaveSession(Session session)
        {
            using var stream = File.Create(TempFile);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, (Session2)session);
        }

        public void ClearSession()
        {
            try
            {
                File.Delete(TempFile);
            }
            catch (FileNotFoundException) { }
        }
    }

    [Serializable]
    public class User2
    {
        Dictionary<string, object> Properties = new Dictionary<string, object>();

        public void AddProperty(string name, object value)
        {
            Properties.Add(name, value);
        }

        public object GetProperty(string name)
        {
            return Properties[name];
        }
        public static implicit operator User2(User user)
        {
            var user2 = new User2();
            foreach (var prop in typeof(User).GetProperties())
            {
                user2.AddProperty(prop.Name, prop.GetValue(user)!);
            }
            return user2;
        }
        public static implicit operator User(User2 user2)
        {
            var user = new User();
            foreach (var prop in typeof(User).GetProperties())
            {
                prop.SetValue(user, user2.GetProperty(prop.Name));
            }
            return user;
        }
    }

    [Serializable]
    public class Session2
    {
        public User2 User;
        public CookieContainer CookieJar;

        public Session2(Session session)
        {
            User = session.User;
            CookieJar = session.CookieJar;
        }

        public static implicit operator Session(Session2 session2)
        {
            return new Session(session2.User, session2.CookieJar);
        }

        public static implicit operator Session2(Session session)
        {
            return new Session2(session);
        }

    }
}