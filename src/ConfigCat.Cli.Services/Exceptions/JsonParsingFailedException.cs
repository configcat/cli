using System;

namespace ConfigCat.Cli.Services.Exceptions;

internal class JsonParsingFailedException(string message, Exception innerException) : Exception(message, innerException);