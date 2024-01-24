namespace Externalities.ParameterModels;

public class FindByEmailParams
{
    public FindByEmailParams(string email)
    {
        this.email = email;
    }

    public string email { get; private set; }
}