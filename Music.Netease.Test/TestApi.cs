using Microsoft.VisualStudio.TestTools.UnitTesting;
using Music.Netease.Models;
using System.Threading.Tasks;

namespace Music.Netease.Test
{
    [TestClass]
    public class TestApi
    {
        static readonly MusicApi api = new MusicApi();
        
        [TestMethod]
        public async Task ShouldSearchSongWork()
        {
            var ret = await api.Search<Song>("A");
            Assert.IsTrue(ret.Items != null);
        }

        [TestMethod]
        public async Task ShouldSearchAlbumWork()
        {
            var ret = await api.Search<Album>("A");
            Assert.IsTrue(ret.Items != null);
        }

        [TestMethod]
        public async Task ShouldSearchArtistWork()
        {
            var ret = await api.Search<Artist>("A");
            Assert.IsTrue(ret.Items != null);
        }

        [TestMethod]
        public async Task ShouldSearchPlaylistWork()
        {
            var ret = await api.Search<Playlist>("A");
            Assert.IsTrue(ret.Items != null);
        }

        [TestMethod]
        public async Task ShouldSearchUserWork()
        {
            var ret = await api.Search<User>("A");
            Assert.IsTrue(ret.Items != null);
        }
    }
}
