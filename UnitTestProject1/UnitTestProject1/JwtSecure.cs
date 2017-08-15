﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTestProject1
{
    public enum JwtHashAlgorithm
    {
        RS256,
        HS384,
        HS512
    }

    public static class JsonWebToken
    {
        private static Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>> HashAlgorithms;

        static JsonWebToken()
        {
            HashAlgorithms = new Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>>
            {
                { JwtHashAlgorithm.RS256, (key, value) => { using (var sha = new HMACSHA256(key)) { return sha.ComputeHash(value); } } },
                { JwtHashAlgorithm.HS384, (key, value) => { using (var sha = new HMACSHA384(key)) { return sha.ComputeHash(value); } } },
                { JwtHashAlgorithm.HS512, (key, value) => { using (var sha = new HMACSHA512(key)) { return sha.ComputeHash(value); } } }
            };
        }

        public static string GenerateToken(string name)
        {
            try
            {
                var key = "JWTJWTJWT";
                //设置超时时间
                int expireMinutes = 60 * 24 * 3;
                var now = DateTime.UtcNow;
                var timeOut = now.AddMinutes(Convert.ToInt32(expireMinutes));

                var payload = new
                {
                    userName = name,
                    timeOut = timeOut.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var token = Encode(payload, key, JwtHashAlgorithm.RS256);
                return token;
            }
            catch (Exception)
            {

            }
            return "";
        }

        public static bool ValidateToken(string token, out string username)
        {
            username = null;
            if (string.IsNullOrEmpty(token))
                return false;
            var parts = token.Split('.');
            if (parts.Length != 3)
                return false;

            try
            {
                var key = "JWTJWTJWT";
                var jsonStr = Decode(token, key);
                var jwtPayLoad = JsonConvert.DeserializeObject<JwtPayload>(jsonStr);
                if (jwtPayLoad == null)
                    return false;
                if (string.IsNullOrEmpty(jwtPayLoad.UserName))
                    return false;
                //检查是否超时
                var now = DateTime.UtcNow;
                var tokenTime = Convert.ToDateTime(jwtPayLoad.TimeOut);
                if (now > tokenTime)
                    return false;

                username = jwtPayLoad.UserName;
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        private static string Encode(object payload, string key, JwtHashAlgorithm algorithm)
        {
            return Encode(payload, Encoding.UTF8.GetBytes(key), algorithm);
        }

        private static string Encode(object payload, byte[] keyBytes, JwtHashAlgorithm algorithm)
        {
            var segments = new List<string>();
            var header = new
            {
                alg = algorithm.ToString(), typ = "JWT"
            };

            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header, Formatting.None));
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload, Formatting.None));

            //计算A
            segments.Add(Base64UrlEncode(headerBytes));
            //计算B
            segments.Add(Base64UrlEncode(payloadBytes));

            var stringToSign = string.Join(".", segments.ToArray());

            var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
            //利用A和B计算C
            byte[] signature = HashAlgorithms[algorithm](keyBytes, bytesToSign);
            segments.Add(Base64UrlEncode(signature));

            return string.Join(".", segments.ToArray());
        }

        private static string Decode(string token, string key)
        {
            return Decode(token, key, true);
        }

        private static string Decode(string token, string key, bool verify)
        {
            var parts = token.Split('.');
            var header = parts[0];
            var payload = parts[1];
            byte[] crypto = Base64UrlDecode(parts[2]);

            var headerJson = Encoding.UTF8.GetString(Base64UrlDecode(header));
            var headerData = JObject.Parse(headerJson);
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
            var payloadData = JObject.Parse(payloadJson);

            //检验
            if (verify)
            {
                var bytesToSign = Encoding.UTF8.GetBytes(string.Concat(header, ".", payload));
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var algorithm = (string)headerData["alg"];

                var signature = HashAlgorithms[GetHashAlgorithm(algorithm)](keyBytes, bytesToSign);
                var decodedCrypto = Convert.ToBase64String(crypto);
                var decodedSignature = Convert.ToBase64String(signature);

                if (decodedCrypto != decodedSignature)
                {
                    throw new ApplicationException(string.Format("Invalid signature. Expected {0} got {1}", decodedCrypto, decodedSignature));
                }
            }

            return payloadData.ToString();
        }

        private static JwtHashAlgorithm GetHashAlgorithm(string algorithm)
        {
            switch (algorithm)
            {
                case "RS256": return JwtHashAlgorithm.RS256;
                case "HS384": return JwtHashAlgorithm.HS384;
                case "HS512": return JwtHashAlgorithm.HS512;
                default: throw new InvalidOperationException("Algorithm not supported.");
            }
        }

        // from JWT spec
        private static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }

        // from JWT spec
        private static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }
    }

    public class JwtPayload
    {
        public string UserName { get; set; }
        public string TimeOut { get; set; }
    }

}
