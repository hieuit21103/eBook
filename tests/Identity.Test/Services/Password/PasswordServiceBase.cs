using Infrastructure.Services;

namespace Identity.Test.Services.Password;

public class PasswordServiceBase
{
    protected readonly PasswordService _passwordService;

    public PasswordServiceBase()
    {
        _passwordService = new PasswordService();
    }
}
