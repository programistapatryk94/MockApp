using Microsoft.EntityFrameworkCore;
using MockApi.Models;

namespace MockApi.Data.Seed.Data
{
    public class DefaultUserCreator
    {
        private readonly AppDbContext _context;

        public DefaultUserCreator(AppDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            var adminUser = _context.Users.IgnoreQueryFilters().FirstOrDefault(p => p.Email == User.AdminEmail);

            if (null == adminUser)
            {
                var user = new User
                {
                    Email = User.AdminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123qwe")
                };
                user.SetNormalizedNames();
                adminUser = _context.Users.Add(user).Entity;
                _context.SaveChanges();
            }
        }
    }
}
