using System;
using System.Security.Cryptography;

namespace ConfigCat.Cli.Tests.Fakes
{
    public class FakeRandomNumberGenerator(int seed = 0) : RandomNumberGenerator
    {
        private Random random = new(seed);

        public override void GetBytes(byte[] data) => this.random.NextBytes(data);
    }
}
