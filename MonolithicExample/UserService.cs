using System.Collections.Generic;
using MonolithicExample.Users;

namespace MonolithicExample.Services
{
    public class UserService
    {
        private readonly List<User> _users = new List<User>();

        public void AddUser(User user)
        {
            if (user.IsValid())
            {
                _users.Add(user);
            }
        }

        public User GetUser(string email)
        {
            return _users.Find(user => user.Email == email);
        }

        public void UpdateUser(User user)
        {
            var existingUser = GetUser(user.Email);
            if (existingUser != null)
            {
                _users.Remove(existingUser);
                _users.Add(user);
            }
        }

        public void DeleteUser(string email)
        {
            var user = GetUser(email);
            if (user != null)
            {
                _users.Remove(user);
            }
        }
    }
}
