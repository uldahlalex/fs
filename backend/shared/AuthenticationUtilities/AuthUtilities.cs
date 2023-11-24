using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace core.AuthenticationUtilities;

public class AuthUtilities
{
    public bool IsJwtValid(string jwt, string secret)
    {
        try
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, new HMACSHA256Algorithm());
        
            var json = decoder.Decode(jwt, secret, verify: true); 
            return true;
        }
        catch
        {
            return false;
        }
    }
    public string IssueJwt(string secret, Dictionary<string, object?> claimsPayload)
    {
        IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
        IJsonSerializer serializer = new JsonNetSerializer();
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
        return encoder.Encode(claimsPayload, secret);
    }


   
}