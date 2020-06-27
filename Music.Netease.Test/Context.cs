using System.Threading.Tasks;
using Music.Netease.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Music.Netease.Test
{
    [TestClass]
    public class Context
    {
        public static MusicApi Api;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            Api = new MusicApi(new Storage());
        }
        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Api.Dispose();
        }
        public static async Task<User> EnsureLogined()
        {
            if (Api.Me == null)
            {
                await Api.LoginAsync(Configuration.Username, Configuration.Password);
            }
            return Api.Me;
        }
    }
}

