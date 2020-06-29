using System.Runtime.InteropServices.ComTypes;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Numerics;
using Music.Netease.Library;

namespace Music.Netease
{
    public static class Encrypt
    {
        static readonly byte[] NONCE = Encoding.UTF8.GetBytes("0CoJUm6Qyw8W8jud");
        static readonly byte[] IV = Encoding.UTF8.GetBytes("0102030405060708");
        static readonly byte[] PCKey = Encoding.UTF8.GetBytes("e82ckenh8dichen8");

        // const string PUBKEY = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDgtQn2JZ34ZC28NWYpAUd98iZ37BUrX/aKzmFbt7clFSs6sXqHauqKWqdtLkF2KexO40H1YTX8z2lSgBBOAxLsvaklV8k4cBFK9snQXE9/DDaFt6Rr7iVZMldczhC0JNgTz+SHXT6CBHuX3e9SdB1Ua44oncaTWz7OBGLbCiK45wIDAQAB";
        const string PUBKEY_M = "00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7";
        const string PUBKEY_E = "010001";


        static Random random = new Random();
        public static Dictionary<string, string> EncryptWebRequest(object data, byte[]? secKey = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException("Data cannot be null");
            };
            var text = data is string ? (string)data : JsonConvert.SerializeObject(data);
            secKey = secKey ?? CreateSecretKey(16);
            // System.IO.File.WriteAllText("/home/kevin/Projects/Fangzi.Telegram.Bot/Music.Netease.Test/fixture/web_encrypt_data.in", text);
            // System.IO.File.WriteAllBytes("/home/kevin/Projects/Fangzi.Telegram.Bot/Music.Netease.Test/fixture/web_encrypt_secKey.in", secKey);
            // System.IO.File.WriteAllText("/home/kevin/Projects/Fangzi.Telegram.Bot/Music.Netease.Test/fixture/web_encrypt_params.out", aesEncrypt(aesEncrypt(text, NONCE, IV), secKey, IV));
            // System.IO.File.WriteAllText("/home/kevin/Projects/Fangzi.Telegram.Bot/Music.Netease.Test/fixture/web_encrypt_encSecKey.out", rsaEncrypt(secKey));

            return new Dictionary<string, string>{
                {"params", aesEncrypt(aesEncrypt(text, NONCE, IV), secKey, IV)},
                {"encSecKey", rsaEncrypt(secKey)}
            };
        }
        public static Dictionary<string, string> EncryptPCRequest(string path, object data)
        {
            var text = data is string ? (string)data : JsonConvert.SerializeObject(data);
            // Ssytem.IO.File.WriteAllText("/home/kevin/Projects/Fangzi.Telegram.Bot/Music.Netease.Test/fixture/pc_encrypt_lyric.in", text);
            var target = path.Replace("/eapi", "/api");
            var message = $"nobody{target}use{text}md5forencrypt";
            var digest = Md5(message);
            var text2 = $"{target}-36cd479b6b5-{text}-36cd479b6b5-{digest}";
            var encBytes = Convert.FromBase64String(aesEncrypt(text2, PCKey, Mode: CipherMode.ECB));
            // System.IO.File.WriteAllText("/home/kevin/Projects/Fangzi.Telegram.Bot/Music.Netease.Test/fixture/pc_encrypt_lyric.out", encBytes.ToHexString());
            return new Dictionary<string, string> {
                { "params", encBytes.ToHexString() }
            };
        }

        public static string DecryptPCResponse(byte[] bytes)
        {
            return aesDecrypt(bytes, PCKey, Mode: CipherMode.ECB);
        }

        static byte[] CreateSecretKey(int size)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return Encoding.UTF8.GetBytes(Enumerable
            .Repeat(chars, size)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
        }

        static string aesEncrypt(string text, byte[] Key, byte[]? IV = null, CipherMode Mode = CipherMode.CBC)
        {
            var aes = Aes.Create();
            var source = Encoding.UTF8.GetBytes(text);

            aes.Key = Key;
            aes.IV = IV ?? Enumerable.Repeat((byte)0, aes.IV.Length).ToArray();
            aes.Mode = Mode;

            using var encryptor = aes.CreateEncryptor();
            var result = encryptor.TransformFinalBlock(source, 0, source.Length);
            return Convert.ToBase64String(result);
            // using var ms = new MemoryStream();
            // using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            // using StreamWriter writer = new StreamWriter(cs);
            // writer.Write(text);
            // writer.Close(); // must close to flush all the data
            // return Convert.ToBase64String(ms.ToArray());
        }

        static string aesDecrypt(byte[] source, byte[] Key, byte[]? IV = null, CipherMode Mode = CipherMode.CBC)
        {
            var aes = Aes.Create();
            aes.Key = Key;
            aes.BlockSize = 128;
            System.IO.File.WriteAllBytes("/home/kevin/Projects/Fangzi.Telegram.Bot/ApiTest/song.dat", source);
            aes.IV = IV ?? Enumerable.Repeat((byte)0, aes.IV.Length).ToArray();
            aes.Mode = Mode;
            using var decryptor = aes.CreateDecryptor();
            var result = decryptor.TransformFinalBlock(source, 0, source.Length);
            return Encoding.UTF8.GetString(result);
        }

        static string rsaEncrypt(IEnumerable<byte> text, string E = PUBKEY_E, string M = PUBKEY_M)
        {
            var hexText = text.Reverse().ToHexString();
            var hexRet = BigInteger.ModPow(
                BigInteger.Parse("0" + hexText, NumberStyles.HexNumber),
                BigInteger.Parse("0" + E, NumberStyles.HexNumber),
                BigInteger.Parse("0" + M, NumberStyles.HexNumber)
            );
            return hexRet.ToString("x2").TrimStart('0').PadLeft(256, '0');
        }

        public static string Md5(string text)
        {
            using var md5 = MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(text);
            byte[] retBs = md5.ComputeHash(bs);
            return retBs.ToHexString("x2");
        }
    }
}