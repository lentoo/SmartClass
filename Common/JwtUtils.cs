using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Model;

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
            //catch (TokenExpiredException)
            //{
            //    return
            //        new { Message = "Token已过期" };
            //}
            catch (SignatureVerificationException)
            {
                return
                   new { Message = "Token无效" };
            }
        }
    }
}
