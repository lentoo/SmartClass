using Common.Cache;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class JwtUtils
    {

        public static string EncodingToken(Payload payload, string secret)
        {
            //secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            //IDateTimeProvider provider = new UtcDateTimeProvider();
            //DateTime now = provider.GetNow().AddDays(7);  //token保存7天

            //DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //double secounds = Math.Round((now - unixEpoch).TotalSeconds);
            //payload.Exp = secounds;
            string token = encoder.Encode(payload, secret);
            return token;
        }
        public static object DecodingToken(string token, string secret)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);
                var json = decoder.Decode(token, secret, verify: true);
                Payload payload = serializer.Deserialize<Payload>(json);
                return payload;
            }
            catch (TokenExpiredException)
            {
                return
                    new { Message = "Token已过期" };
            }
            catch (SignatureVerificationException)
            {
                return
                   new { Message = "Token无效" };
            }
        }
        /// <summary>
        /// 生成一个Token
        /// </summary>
        /// <param name="User">用户实体</param>
        /// <param name="UserLogOn">用户实体</param>
        /// <param name="text">文本</param>
        /// <returns>token</returns>
        //public static string CreateToken(Sys_User User, Sys_UserLogOn UserLogOn, string text)
        //{
        //    header header = new header
        //    {
        //        alg = "DES",
        //        typ = "JWT"
        //    };

        //    Payload Payload = new Payload
        //    {
        //        Account = User.F_Account,
        //        iat = DateTimeHelper.GetTotoalMilliseconds(),
        //        exp = DateTimeHelper.GetTotoalMilliseconds(DateTime.Now.AddDays(7))
        //    };
        //    //TODO User.F_Account最后要改成text
        //    string token = Base64Helper.Encry(header) + "." + Base64Helper.Encry(Payload) + "." + Base64Helper.Encry(DESEncrypt.Encrypt(text, UserLogOn.F_UserSecretkey));
        //    return token;
        //}
        //public static bool DecryToken(string token)
        //{
        //    string[] ss = token.Split('.');

        //    if (ss.Length == 3)
        //    {
        //        header h = Base64Helper.Decry<header>(ss[0]);
        //        Payload p = Base64Helper.Decry<Payload>(ss[1]);
        //        object t = CacheHelper.GetCache(p.Account);
        //        if (t == null)//缓存失效
        //        {
        //            return false;
        //        }
        //        string s = Base64Helper.Decry(ss[2]);
        //        if (DateTimeHelper.GetTotoalMilliseconds(DateTime.Now) - p.exp > 0) //令牌过期
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}
