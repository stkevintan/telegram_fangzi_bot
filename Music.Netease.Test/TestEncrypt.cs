using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Music.Netease.Test
{
    [TestClass]
    public class TestEncrypt
    {

        string getPath(string filename) => Path.Join(Configuration.Root, "fixture", filename);

        [TestMethod]
        public void ShouldPCDecryptWork()
        {
            var bytes = File.ReadAllBytes(getPath("pc_decrypt_lyric.in"));
            var actual = Encrypt.DecryptPCResponse(bytes);
            var expect = File.ReadAllText(getPath("pc_decrypt_lyric.out"));
            Assert.AreEqual(actual, expect);
        }

        [TestMethod]
        public void ShouldPCEncryptWork()
        {
            var text = File.ReadAllText(getPath("pc_encrypt_lyric.in"));
            var actual = Encrypt.EncryptPCRequest("/eapi/song/lyric", text)["params"];
            var expect = File.ReadAllText(getPath("pc_encrypt_lyric.out"));
            Assert.AreEqual(actual, expect);
        }

        [TestMethod]
        public void ShouldWebEncryptWork()
        {
            var data = File.ReadAllText(getPath("web_encrypt_data.in"));
            var secKey = File.ReadAllBytes(getPath("web_encrypt_secKey.in"));
            var actual = Encrypt.EncryptWebRequest(data, secKey);
            Assert.AreEqual(actual["params"], File.ReadAllText(getPath("web_encrypt_params.out")));
            Assert.AreEqual(actual["encSecKey"], File.ReadAllText(getPath("web_encrypt_encSecKey.out")));
        }
    }
}