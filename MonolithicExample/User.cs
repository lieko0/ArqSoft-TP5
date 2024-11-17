using MonolithicExample.Services;

namespace MonolithicExample.Users
{
    public class User
    {
        public string Name { get; }
        public string Email { get; }
        public bool IsAdmin { get; }
        public string Password { get; }

        public User(string name, string email, string password, bool isAdmin = false)
        {
            Name = name;
            Email = email;
            Password = password;
            IsAdmin = isAdmin;
        }

        public bool IsValid()
        {
            return IsValidService.IsValid(this);
        }

        public override string ToString()
        {
            return $"{Name} <{Email}>";
        }
    }
}
