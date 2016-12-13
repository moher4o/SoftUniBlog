namespace Blog.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class Configuration : DbMigrationsConfiguration<Blog.Models.BlogDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Blog.Models.ApplicationDbContext";
        }

        protected override void Seed(Blog.Models.BlogDbContext context)
        {
            if (!context.Roles.Any())
            {
                this.CreateRole("Admin", context);
                this.CreateRole("User", context);
            }
            if (!context.Users.Any())
            {
                this.CreateUser("admin@admin.com", "Admin", "123", context);
                this.SetUserRole("admin@admin.com", "Admin", context);

                this.CreateUser("gosho@abv.com", "Gosho", "123", context);
                this.SetUserRole("gosho@abv.com", "User", context);

            }
        }

        private void CreateRole(string roleName, BlogDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>
                (new RoleStore<IdentityRole>(context));

            var result = roleManager.Create(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }

        }

        private void SetUserRole(string email, string role, BlogDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>
                (new UserStore<ApplicationUser>(context));

            var userId = context.Users.FirstOrDefault(u => u.Email.Equals(email)).Id;

            var result = userManager.AddToRole(userId, role);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }

        }

        private void CreateUser(string email, string fullName, string pass, BlogDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>
                (new UserStore<ApplicationUser>(context));

            userManager.PasswordValidator = new PasswordValidator
            {
                RequireDigit = false,
                RequiredLength = 1,
                RequireLowercase = false,
                RequireNonLetterOrDigit = false,
                RequireUppercase = false,
            };

            var user = new ApplicationUser()
            {
                Email = email,
                FullName = fullName,
                UserName = email,
            };

           var result =  userManager.Create(user, pass);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }
        }
    }
}
