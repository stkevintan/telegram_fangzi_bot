using System.Net.Http;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Music.Netease.Models;
using System.Threading.Tasks;

namespace Music.Netease.Test
{
    [TestClass]
    public class TestLogin
    {
        static readonly MusicApi api = new MusicApi();

        public static async Task<User> DoLogin()
        {
            try
            {
                return await api.LoginAsync(Configuration.Username, Configuration.Password);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
                return null;
            }
        }

        [TestMethod]
        public async Task ShouldCellphoneLoginSuccess()
        {
            var user = await DoLogin();
            Assert.AreEqual(user.Name, Configuration.Nickname);
        }

        [TestMethod]
        public async Task ShouldCookieMaintainered()
        {
            var user = await DoLogin();
            var ret = await api.SearchAsync<Song>("A");
        }
    }
}
