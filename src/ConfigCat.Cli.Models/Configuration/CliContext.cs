using System;
using System.Net.Http.Headers;

namespace ConfigCat.Cli.Models.Configuration;

public class CliContext
{
    public AuthenticationHeaderValue AuthenticationHeader { get; private set; }
    
    public string ApiHost { get; set; }
    
    public Workspace Workspace { get; set; }
    
    public void SetBasicAuth(string userName, string password)
    {
        AuthenticationHeader = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{userName}:{password}")));
    }
}