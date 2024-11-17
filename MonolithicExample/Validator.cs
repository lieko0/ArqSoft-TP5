using MonolithicExample.Users;
using MonolithicExample.Services;

namespace MonolithicExample.Validation
{
    public static class Validator
    {
        public static bool Validate(User user)
        {
            return ValidateService.Validate(user);
        }
    }
}
