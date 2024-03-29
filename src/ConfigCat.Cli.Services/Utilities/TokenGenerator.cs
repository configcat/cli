using System;
using System.Security.Cryptography;

namespace ConfigCat.Cli.Services.Utilities
{
    public interface ITokenGenerator
    {
        string GenerateTokenString(int length);

        byte[] GenerateToken(int length);
    }

    public class TokenGenerator(RandomNumberGenerator randomNumberGenerator) : ITokenGenerator
    {
        public byte[] GenerateToken(int length)
        {
            var bytes = new byte[length];
            randomNumberGenerator.GetBytes(bytes);
            return bytes;
        }

        public string GenerateTokenString(int length)
        {
            var bytes = new byte[length];
            randomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}