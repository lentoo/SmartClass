using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JwtDemo
{
    class Program
    {
        static void Main(string[] args)
        {
           
            var secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            IDateTimeProvider provider = new UtcDateTimeProvider();
            DateTime now = provider.GetNow().AddDays(7);
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            double secounds = Math.Round((now - unixEpoch).TotalSeconds);
            var payload = new Dictionary<string, object>
            {
                { "Account", 110 },
                { "claim2", "claim2-value" },
                { "exp",secounds}
            };
            var token = encoder.Encode(payload, secret);

            Console.WriteLine(token);
            Decoding(token);

            Console.ReadKey();
        }
        static void Decoding(string token)
        {
            //var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJjbGFpbTEiOjAsImNsYWltMiI6ImNsYWltMi12YWx1ZSJ9.8pwBI_HtXqI3UgQHQ_rDRnSQRxFL1SR8fbQoS-5kM5s";
            var secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

                var json = decoder.Decode(token, secret, verify: true);
                obj o =  serializer.Deserialize<obj>(json);
                Console.WriteLine(json);
                Console.WriteLine(o.Account);
                Console.WriteLine(o.claim2);
            }
            catch (TokenExpiredException)
            {
                Console.WriteLine("Token has expired");
            }
            catch (SignatureVerificationException)
            {
                Console.WriteLine("Token has invalid signature");
            }
        }
    }
    public class obj
    {
        public int Account { get; set; }
        public string claim2 { get; set; }
    }
}
