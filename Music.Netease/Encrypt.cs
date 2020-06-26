using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Numerics;
using Music.Netease.Models;

namespace Music.Netease
{
    public class Encrypt
    {
        static readonly byte[] NONCE = Encoding.UTF8.GetBytes("0CoJUm6Qyw8W8jud");
        static readonly byte[] IV = Encoding.UTF8.GetBytes("0102030405060708");

        // const string PUBKEY = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDgtQn2JZ34ZC28NWYpAUd98iZ37BUrX/aKzmFbt7clFSs6sXqHauqKWqdtLkF2KexO40H1YTX8z2lSgBBOAxLsvaklV8k4cBFK9snQXE9/DDaFt6Rr7iVZMldczhC0JNgTz+SHXT6CBHuX3e9SdB1Ua44oncaTWz7OBGLbCiK45wIDAQAB";
        const string PUBKEY_M = "00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7";
        const string PUBKEY_E = "010001";


        static Random random = new Random();
        public EncryptedBody EncrptedRequest(object data)
        {
            if (data == null) {
                throw new ArgumentNullException("Data cannot be null");
            };
            var text = JsonConvert.SerializeObject(data);
            var secKey = createSecretKey(16);
            return new EncryptedBody()
            {
                @params = aesEncrypt(aesEncrypt(text, NONCE, IV), secKey, IV),
                encSecKey = rsaEncrypt(secKey)
            };
        }

        byte[] createSecretKey(int size)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return Encoding.UTF8.GetBytes(Enumerable
            .Repeat(chars, size)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
        }

        string aesEncrypt(string text, byte[] Key, byte[] IV, CipherMode Mode = CipherMode.CBC)
        {
            var aes = Aes.Create();
            var source = Encoding.UTF8.GetBytes(text);

            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;

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

        string rsaEncrypt(IEnumerable<byte> text, string E = PUBKEY_E, string M = PUBKEY_M)
        {
            var hexText = string.Join("", text.Reverse().Select(c => ((int)c).ToString("X2")));
            var hexRet = BigInteger.ModPow(
                BigInteger.Parse("0" + hexText, NumberStyles.HexNumber),
                BigInteger.Parse("0" + E, NumberStyles.HexNumber),
                BigInteger.Parse("0" + M, NumberStyles.HexNumber)
            );
            return hexRet.ToString("x2").TrimStart('0').PadLeft(256, '0');
        }

        public string Md5(string text) {
            using var md5 = MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(text);
            byte[] retBs = md5.ComputeHash(bs);
            return String.Join("", from b in retBs select ((int)b).ToString("x2"));
        }
    }

}