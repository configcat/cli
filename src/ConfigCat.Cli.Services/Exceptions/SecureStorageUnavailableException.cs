using System;

namespace ConfigCat.Cli.Services.Exceptions;

public class SecureStorageUnavailableException(string message = "Could not access secure storage.", Exception innerException = null) : Exception(message, innerException);