using System.Collections.Generic;
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
            // await Context.EnsureLogined();
            // ALiz: 29307041
            var ret = await Context.Api.SongUrlAsync(29307041);
            var ret2 = await Context.Api.SongUrlAsync(29307041, (int)SongQuality.HD);
            Assert.AreEqual(ret.Quality, ret2.Quality);
            Assert.IsNull(ret.FreeTrialInfo);

            var ret3 = await Context.Api.SongUrlAsync(new List<long>() { 29307041, 29829683 });
            Assert.IsTrue(ret3 is List<SongUrl>);

            //vip
            var ret4 = await Context.Api.SongUrlAsync(26127499);
            Assert.IsNotNull(ret4.FreeTrialInfo);

        }

        [TestMethod]
        public async Task ShouldSongLyricWork()
        {
            // await Context.EnsureLogined();
            var ret = await Context.Api.SongLyricAsync(29829683);
        }

        [TestMethod]
        public async Task ShouldSongDetailWork()
        {
            var ret = await Context.Api.SongDetail(29829683);
            Assert.IsNotNull(ret);
            var rlist = await Context.Api.SongDetail(new List<long>() { 29829683, 29307041 });
            Assert.IsTrue(rlist.Count == 2);
        }

        [TestMethod]
        public async Task ShouldAlbumDetailWork()
        {
            var ret = await Context.Api.AlbumDetail(3086101);
            Assert.IsTrue(ret.Songs.Count > 0);
        }

        [TestMethod]
        public async Task ShouldArtistAlbumWork() {
            var ret = await Context.Api.ArtistAlbum(15988, 2);
            Assert.IsTrue(ret.Count > 0);
        }

        [TestMethod]
        public async Task ShouldArtistDescWork() {
            var ret = await Context.Api.ArtistDesc(15988);
        }
    }
}
