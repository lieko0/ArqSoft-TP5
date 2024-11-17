using System;
using MonolithicExample.Users;

namespace MonolithicExample.Services
{
    public static class MainService
    {
        public static void Main(string[] args)
        {
            var userService = new UserService();
            var user = new User("John Doe", "john.doe@example.com", "password123");
            userService.AddUser(user);

            var admin = new AdminUser("Admin", "admin@example.com", "adminpassword");
            userService.AddUser(admin);

            Console.WriteLine(user.ToString());
            Console.WriteLine(admin.ToString());

            if (AuthenticationService.Authenticate(user, "password123"))
            {
                Console.WriteLine("User authenticated successfully.");
            }

            if (AuthorizationService.Authorize(admin, "AdminTask"))
            {
                admin.PerformAdminTask();
                Console.WriteLine("Admin task performed.");
            }
        }
    }
}
