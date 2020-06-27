using Microsoft.VisualStudio.TestTools.UnitTesting;
using Music.Netease.Models;
using System.Threading.Tasks;

namespace Music.Netease.Test
{
    [TestClass]
    public class TestLogin
    {
        [TestMethod]
        public async Task ShouldCellphoneLoginSuccess()
        {
            var api = new MusicApi();
            var user = await api.LoginAsync(Configuration.Username, Configuration.Password);
            Assert.AreEqual(user.Name, Configuration.Nickname);
        }

        [TestMethod]
        public async Task ShouldCookiePersist()
        {
            var storage = new Storage();
            var api = new MusicApi(storage);
            var user = api.Me ?? await api.LoginAsync(Configuration.Username, Configuration.Password);
            api.Dispose();
            api = new MusicApi(storage);
            Assert.IsNotNull(api.Me);
            await api.RecommendAsync<Song>();
        }

        [TestMethod]
        public async Task ShouldCookieMaintainered()
        {
            await Context.EnsureLogined();
            var ret = await Context.Api.SearchAsync<Song>("A");
        }
    }
}
