﻿using System;
using System.Security.Cryptography;

namespace ConfigCat.Cli.Services.Utilities
{
    public interface ITokenGenerator
    {
        string GenerateTokenString(int length);

        byte[] GenerateToken(int length);
    }

    public class TokenGenerator : ITokenGenerator
    {
        private readonly RandomNumberGenerator randomNumberGenerator;

        public TokenGenerator(RandomNumberGenerator randomNumberGenerator)
        {
            this.randomNumberGenerator = randomNumberGenerator;
        }

        public byte[] GenerateToken(int length)
        {
            var bytes = new byte[length];
            this.randomNumberGenerator.GetBytes(bytes);
            return bytes;
        }

        public string GenerateTokenString(int length)
        {
            var bytes = new byte[length];
            this.randomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}