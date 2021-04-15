using System;
using System.Net;

namespace ConfigCat.Cli.Exceptions
{
    class HttpStatusException : Exception
    {
        public HttpStatusException(HttpStatusCode statusCode, 
            string reason,
            string message = null, 
            Exception innerException = null) : base(message, innerException)
        {
            this.StatusCode = statusCode;
            this.ReasonPhrase = reason;
        }

        public HttpStatusCode StatusCode { get; set; }

        public string ReasonPhrase { get; set; }
    }
}
