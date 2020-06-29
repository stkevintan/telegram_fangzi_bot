using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Music.Netease.Models;
using System.Threading.Tasks;

namespace Music.Netease.Test
{
    [TestClass]
    public class TestApi
    {
        [TestMethod]
        public async Task ShouldSearchSongWork()
        {
            var ret = await Context.Api.SearchAsync<Song>("A");
            Assert.IsNotNull(ret.Items);
        }

        [TestMethod]
        public async Task ShouldSearchAlbumWork()
        {
            var ret = await Context.Api.SearchAsync<Album>("A");
            Assert.IsNotNull(ret.Items);
        }

        [TestMethod]
        public async Task ShouldSearchArtistWork()
        {
            var ret = await Context.Api.SearchAsync<Artist>("A");
            Assert.IsNotNull(ret.Items);
        }

        [TestMethod]
        public async Task ShouldSearchPlaylistWork()
        {
            var ret = await Context.Api.SearchAsync<Playlist>("A");
            Assert.IsNotNull(ret.Items);
        }

        [TestMethod]
        public async Task ShouldSearchUserWork()
        {
            var ret = await Context.Api.SearchAsync<User>("A");
            Assert.IsNotNull(ret.Items);
        }

        [TestMethod]
        public async Task ShouldDailyTaskWork()
        {
            await Context.EnsureLogined();
            try
            {
                var point = await Context.Api.DailyTaskAsync();
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
            await Context.EnsureLogined();
            await Context.Api.RecommendAsync<Playlist>();
            await Context.Api.RecommendAsync<Song>();
        }

        [TestMethod]
        public async Task ShouldRecommendNotWorkWithoutLogin()
        {
            var api = new MusicApi();
            await Assert.ThrowsExceptionAsync<HttpRequestException>(api.RecommendAsync<Playlist>);
            api.Dispose();
        }

        [TestMethod]
        public async Task ShouldUserPlaylistWork()
        {
            var user = await Context.EnsureLogined();
            var plist = await Context.Api.UserPlaylistAsync(user.Id);
        }

        [TestMethod]
        public async Task ShouldPersonalFmWork()
        {
            await Context.EnsureLogined();
            var ret = await Context.Api.PersonalFmAsync();
            Assert.IsTrue(ret.Count > 0);
        }

        [TestMethod]
        public async Task ShouldSongUrlWork()
        {
            await Context.EnsureLogined();
            // ALiz: 29307041
            //vip: 26127499
            await Context.Api.SongUrlAsync(29307041);
        }

        [TestMethod]
        public async Task ShouldSongLyricWork() {
            await Context.EnsureLogined();
            await Context.Api.SongLyricAsync(29829683);
        }
    }
}
