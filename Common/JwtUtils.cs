using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Model;
using Model.DTO.Result;
using Model.Enum;
using System;

namespace SmartClass.Infrastructure
{
  public class JwtUtils
  {
    static string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
    public static string EncodingToken(Payload payload)
    {


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
    public static ValidateTokenResult DecodingToken(string token)
    {
      ValidateTokenResult validateTokenResult = new ValidateTokenResult();
      try
      {
        IJsonSerializer serializer = new JsonNetSerializer();
        IDateTimeProvider provider = new UtcDateTimeProvider();
        IJwtValidator validator = new JwtValidator(serializer, provider);
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);
        var json = decoder.Decode(token, secret, verify: true);
        Payload payload = serializer.Deserialize<Payload>(json);
        var currentTime = DateTime.Now;
        if(currentTime > payload.Exp)
        {
          throw new TokenExpiredException("Token已过期");
        }
        else  //未过期，延长时间
        {
          payload.Exp = DateTime.Now.AddDays(7);          
        }
        validateTokenResult.Status = true;
        validateTokenResult.Payload = payload;
        validateTokenResult.ResultCode = ResultCode.Ok;

        return validateTokenResult;
      }
      catch (TokenExpiredException ex)
      {
        validateTokenResult.Status = false;
        validateTokenResult.ErrorData = ex.Message;
        validateTokenResult.ResultCode = ResultCode.Error;
        return validateTokenResult;
      }
      catch (SignatureVerificationException exception)
      {
        validateTokenResult.Status = false;
        validateTokenResult.ErrorData = exception;
        validateTokenResult.ResultCode = ResultCode.Error;
        return validateTokenResult;
      }
    }
  }
}
