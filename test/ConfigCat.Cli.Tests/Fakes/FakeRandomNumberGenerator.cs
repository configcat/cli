using System;
using System.Security.Cryptography;

namespace ConfigCat.Cli.Tests.Fakes
{
    public class FakeRandomNumberGenerator : RandomNumberGenerator
    {
        private Random random;

        public FakeRandomNumberGenerator(int seed = 0)
        {
            this.random = new Random(seed);
        }

        public override void GetBytes(byte[] data) => this.random.NextBytes(data);
    }
}
