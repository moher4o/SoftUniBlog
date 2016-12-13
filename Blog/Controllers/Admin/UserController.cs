using Blog.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            using (var db = new BlogDbContext())
            {
                var users = db.Users
                            .ToList();

                ViewBag.Admins = GetAdmins(db);

                return View(users);
            }
        }

        private HashSet<string> GetAdmins(BlogDbContext db)
        {
            var userManager = Request
                .GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            var users = db.Users.ToList();

            var admins = new HashSet<string>();
            foreach (var user in users)
            {
                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    admins.Add(user.Id);
                }
            }
            return admins;
        }

        
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id.Equals(id));

                var model = new UserViewModel();
                model.Email = user.Email;
                model.FullName = user.FullName;
                model.Roles = GetUserRoles(user, db);

                return View(model);
            }
                
        }

        [HttpPost]
        public ActionResult Edit(string id, UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BlogDbContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id.Equals(id));

                    if (user == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }

                    user.Email = model.Email;
                    user.FullName = model.FullName;
                    user.UserName = model.Email;

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var passwordHasher = new PasswordHasher();
                        var newPasswordHash = passwordHasher.HashPassword(model.Password);
                        user.PasswordHash = newPasswordHash;
                    }

                    SetUserRoles(user, db, model);

                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                    return RedirectToAction("List");
            }


            return View(model);
        }

        public ActionResult Delete(string id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id.Equals(id));

                if (user == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                return View(user);
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            using (var db = new BlogDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id.Equals(id));
                var article = db.Articles.FirstOrDefault(a => a.AuthorId.Equals(user.Id));
                
                    if (article != null)
                    {
                         return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }

                
                db.Users.Remove(user);
                db.SaveChanges();
                return RedirectToAction("List");
            }
        } 
        

        private void SetUserRoles(ApplicationUser user, BlogDbContext db, UserViewModel model)
        {
            var userManager = Request
                              .GetOwinContext()
                              .GetUserManager<ApplicationUserManager>();

            foreach (var role in model.Roles)
            {
                if (role.IsSelected)
                {
                    userManager.AddToRole(user.Id, role.Name);
                }
                else if (!role.IsSelected)
                {
                    userManager.RemoveFromRoles(user.Id, role.Name);
                }

            }
        }

        private List<Role> GetUserRoles(ApplicationUser user, BlogDbContext db)
        {
            var rolesInDatabase = db.Roles
                .Select(r => r.Name)
                .OrderBy(r => r)
                .ToList();

            var userManager = Request
                .GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            List<Role> userRoles = new List<Role>();

            foreach (var roleName in rolesInDatabase)
            {
                Role role = new Role() { Name = roleName};

                if (userManager.IsInRole(user.Id, roleName))
                {
                    role.IsSelected = true;
                }

                userRoles.Add(role);  
            }
            return userRoles;
        }
    }
}