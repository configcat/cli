using System;

namespace ConfigCat.Cli.Services.Exceptions;

public class ShowHelpException(string message) : Exception(message);