using MonolithicExample.Users;
using MonolithicExample.Validation;

namespace MonolithicExample.Services
{
    public static class IsValidService
    {
        public static bool IsValid(User user)
        {
            return Validator.Validate(user);
        }
    }
}
