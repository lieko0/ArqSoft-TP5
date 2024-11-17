namespace MonolithicExample.Users
{
    public class AdminUser : User
    {
        public AdminUser(string name, string email, string password) : base(name, email, password, true)
        {
        }

        public void PerformAdminTask()
        {
            // Admin-specific task
        }
    }
}
