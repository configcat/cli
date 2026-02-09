using System;
using System.Collections.Generic;
using System.Net;

namespace ConfigCat.Cli.Services.Exceptions;

public class HttpStatusException(
    HttpStatusCode statusCode,
    string reason,
    IReadOnlyDictionary<string, string[]> errorDetails = null,
    string message = null,
    Exception innerException = null)
    : Exception(message, innerException)
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public string ReasonPhrase { get; } = reason;

    public IReadOnlyDictionary <string, string[]> ErrorDetails { get; } = errorDetails;
}