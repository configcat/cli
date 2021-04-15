using System;

namespace ConfigCat.Cli.Exceptions
{
    class ShowHelpException : Exception
    {
        public ShowHelpException(string message)
            : base(message)
        { }
    }
}
