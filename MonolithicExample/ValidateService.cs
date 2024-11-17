using MonolithicExample.Users;

namespace MonolithicExample.Services
{
    public static class ValidateService
    {
        public static bool Validate(User user)
        {
            return !string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(user.Password);
        }
    }
}
