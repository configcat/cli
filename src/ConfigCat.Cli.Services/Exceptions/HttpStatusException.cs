using System;
using System.Net;

namespace ConfigCat.Cli.Services.Exceptions;

public class HttpStatusException(
    HttpStatusCode statusCode,
    string reason,
    string message = null,
    Exception innerException = null)
    : Exception(message, innerException)
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public string ReasonPhrase { get; } = reason;
}