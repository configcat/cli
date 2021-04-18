using System;

namespace ConfigCat.Cli.Services.Exceptions
{
    public class ShowHelpException : Exception
    {
        public ShowHelpException(string message)
            : base(message)
        { }
    }
}
