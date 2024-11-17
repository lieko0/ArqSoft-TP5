using MonolithicExample.Users;

namespace MonolithicExample.Services
{
    public static class AuthorizationService
    {
        public static bool Authorize(User user, string task)
        {
            if (user.IsAdmin && task == "AdminTask")
            {
                return true;
            }
            return false;
        }
    }
}
