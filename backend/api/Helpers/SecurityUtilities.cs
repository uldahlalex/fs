using System.Security.Cryptography;
using System.Text;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json;
using Serilog;

namespace api.Helpers;

public static class SecurityUtilities
{
    public static Dictionary<string, string> ValidateJwtAndReturnClaims(string jwt)
    {
        try
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, new HMACSHA256Algorithm());
            var json = decoder.Decode(jwt, Environment.GetEnvironmentVariable("FULLSTACK_JWT_PRIVATE_KEY"));
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;
        }
        catch
        {
            throw new Exception("Authentication failed.");
        }
    }


    public static string IssueJwt(Dictionary<string, object> claimsPayload)
    {
        try
        {
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            return encoder.Encode(claimsPayload, Environment.GetEnvironmentVariable("FULLSTACK_JWT_PRIVATE_KEY"));
        }
        catch (Exception e)
        {
            Log.Error(e, "IssueJWT");
            throw new Exception("User authentication succeeded, but could not create token");
        }
    }

    public static string Hash(string password, string salt)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        catch (Exception e)
        {
            Log.Error(e, "Hash");
            throw new Exception("Failed to hash password");
        }
    }

    public static string GenerateSalt()
    {
        var bytes = new byte[128 / 8];
        using var keyGenerator = RandomNumberGenerator.Create();
        keyGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}