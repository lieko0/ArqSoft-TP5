using MonolithicExample.Users;

namespace MonolithicExample.Services
{
    public static class AuthenticationService
    {
        public static bool Authenticate(User user, string password)
        {
            return user.Password == password;
        }
    }
}
