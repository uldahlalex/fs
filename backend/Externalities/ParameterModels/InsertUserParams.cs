namespace Externalities.ParameterModels;

public class InsertUserParams
{
    public InsertUserParams(string email, string hash, string salt)
    {
        this.email = email;
        this.hash = hash;
        this.salt = salt;
    }

    public string email { get; private set; }
    public string hash { get; private set; }
    public string salt { get; private set; }
}