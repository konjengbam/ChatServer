using System;

namespace ChatServer
{
    class User
    {
        public User(string id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }

        public string Id { get; }
        public string Name { get; }
        public string Email { get; }

        public override string ToString()
        {
            return "User: " + Id + " " + Name + " " + Email;
        }

        public override bool Equals(object obj)
        {
            return obj is User user &&
                   Id == user.Id &&
                   Name == user.Name &&
                   Email == user.Email;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Email);
        }
    }
}
