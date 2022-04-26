using System;

namespace ConfigCat.Cli.Services.Exceptions
{
    internal class JsonParsingFailedException : Exception
    {
        public JsonParsingFailedException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}