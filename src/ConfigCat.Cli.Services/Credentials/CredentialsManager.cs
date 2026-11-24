namespace ConfigCat.Cli.Services.Credentials;

public interface ICredentialsManager
{
    ICredentialStore CreateBasicAuthCredentialStore();
}

public class CredentialsManager : ICredentialsManager
{
    public ICredentialStore CreateBasicAuthCredentialStore()
    {
        return new CredentialStore("basic_auth");
    }
}