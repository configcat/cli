namespace ConfigCat.Cli.Services.Credentials;

public interface ICredentialsManager
{
    ICredentialStore CreateApiCredentialStore();
}

public class CredentialsManager : ICredentialsManager
{
    public ICredentialStore CreateApiCredentialStore()
    {
        return new CredentialStore("apicred");
    }
}