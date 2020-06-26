using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Music.Netease.Models;
using System.Threading.Tasks;

namespace Music.Netease.Test
{
    [TestClass]
    public class TestSearch
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
        public async Task ShouldSearchSongWork()
        {
            var ret = await api.SearchAsync<Song>("A");
            Assert.IsNotNull(ret.Items);
        }

        [TestMethod]
        public async Task ShouldSearchAlbumWork()
        {
            var ret = await api.SearchAsync<Album>("A");
            Assert.IsNotNull(ret.Items);
        }

        [TestMethod]
        public async Task ShouldSearchArtistWork()
        {
            var ret = await api.SearchAsync<Artist>("A");
            Assert.IsNotNull(ret.Items);
        }

        [TestMethod]
        public async Task ShouldSearchPlaylistWork()
        {
            var ret = await api.SearchAsync<Playlist>("A");
            Assert.IsNotNull(ret.Items);
        }

        [TestMethod]
        public async Task ShouldSearchUserWork()
        {
            var ret = await api.SearchAsync<User>("A");
            Assert.IsNotNull(ret.Items);
        }

        [TestMethod]
        public async Task ShouldDailyTaskWork()
        {
            await DoLogin();
            try
            {
                var point = await api.DailyTaskAsync();
                Assert.IsTrue(point > 0);
            }
            catch (HttpRequestException e)
            {
                Assert.AreEqual(e.Data["Code"], -2);
            }
        }

        [TestMethod]
        public async Task ShouldRecommendWork()
        {
            await DoLogin();
            try
            {
                await api.RecommendAsync<Playlist>();
                await api.RecommendAsync<Song>();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public async Task ShouldRecommendNotWorkWithoutLoginAsync()
        {
            await Assert.ThrowsExceptionAsync<HttpRequestException>(api.RecommendAsync<Playlist>);
        }
    }
}
