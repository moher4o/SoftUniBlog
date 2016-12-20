using Blog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var articles = database.Articles
                    .Include(a => a.Author)
                    .Include(t => t.Tags)
                    .ToList();
                return View(articles);
            }
                
            
        }

        public ActionResult ListUserArticles(string id)
        {
            using (var db = new BlogDbContext())
            {
                var articles = db.Articles
                    .Include(a => a.Author)
                    .Include(t => t.Tags)
                    .Include(c => c.Category)
                    .Where(u => u.AuthorId.Equals(id))
                    .ToList();
                return View(articles);
            }

        }

        public ActionResult Details(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles
                    .Include(a => a.Author)
                    .Include(t => t.Tags)
                    .Include(c => c.Comments)
                    .Include(i => i.Images)
                    .Where(a => a.Id == id)
                    .FirstOrDefault();

                var user = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name));
                if (user == null || article.AuthorId != user.Id)
                {
                    article.VisitCounter += 1;
                    db.Entry(article).State = EntityState.Modified;
                    db.SaveChanges();
                }


                return View(article);
            }

            
        }
        //Get
        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            using (var db = new BlogDbContext())
            {
                var model = new ArticleViewModel();
                model.Categories = db.Categories.ToList();
                return View(model);
            }
           
        }

        //Post
        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(ArticleViewModel model)
        {
            
            using (var db = new BlogDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name));

                var article = new Article(user.Id, model.Title, model.Content, model.CategoryId);
                //article.AuthorId = user.Id;
                //article.Title = model.Title;
                //article.Content = model.Content;
                //article.CategoryId = model.CategoryId;
               
                this.SetArticleTags(article, model, db);

                db.Articles.Add(article);
                db.SaveChanges();
            //if (file != null)
            //{
            //        var image = new ArticleImage();

            //        string pic = System.IO.Path.GetFileName(file.FileName);
            //    pic = "new" + pic;
            //    string path = System.IO.Path.Combine(
            //                           Server.MapPath("~/images"), pic);
            //    // file is uploaded
            //    file.SaveAs(path);
            //        image.ArticleId = article.Id;
            //        image.FileName = file.FileName;
                    

            //    // save the image path path to the database or you can send image 
            //    // directly to database
            //    // in-case if you want to store byte[] ie. for DB
            //    using (MemoryStream ms = new MemoryStream())
            //    {
            //        file.InputStream.CopyTo(ms);
            //        byte[] array = ms.GetBuffer();
            //    }


            //}
            //// after successfully uploading redirect the user
            return RedirectToAction("ListUserArticles", new { user.Id });
            }

        }


        //Get
        [HttpGet]
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if(id== null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.FirstOrDefault(a => a.Id == id);

                if (!this.IsAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

                }
                if (article == null)
                {
                    return HttpNotFound();
                }

                var model = new ArticleViewModel();
                model.AuthorId = article.AuthorId;
                model.Title = article.Title;
                model.Content = article.Content;
                model.CategoryId = article.CategoryId;
                model.Categories = db.Categories.ToList();
                model.Tags = string.Join(", ", article.Tags.Select(t => t.Name));

                return View(model);
            }
        }


        //Post
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BlogDbContext())
                {

                    var article = db.Articles.FirstOrDefault(a => a.Id == id);
                    article.Title = model.Title;
                    article.Content = model.Content;
                    article.CategoryId = model.CategoryId;
                    article.AuthorId = model.AuthorId;
                    this.SetArticleTags(article, model, db);

                    db.Entry(article).State = EntityState.Modified;
                    db.SaveChanges();

                    // return RedirectToAction("List");
                    return RedirectToAction("Details", new { id });
                }
            }

            return View(model);
        }

        //Get
        [HttpGet]
        [Authorize]
        public ActionResult Delete(int? id)
        {
            using (var db = new BlogDbContext())
            {
                var article = db.Articles
                    .Include(a => a.Category)
                    .Include(a => a.Tags)
                    .FirstOrDefault(m => m.Id == id);

                ViewBag.Tags = string.Join(", ", article.Tags.Select(t => t.Name));
                if(article == null)
                {
                    return HttpNotFound();
                }

                return View(article);
            }
                
        }

        [HttpPost]
        [Authorize]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var db = new BlogDbContext())
            {
                var article = db.Articles.FirstOrDefault(m => m.Id == id);

                if (article == null)
                {
                    return HttpNotFound();
                }

                db.Articles.Remove(article);
                db.SaveChanges();

                return RedirectToAction("ListCategories", "Home");
            }

        }

        private bool IsAuthorizedToEdit(Article article)
        {
            bool isAuthor = article.IsUserAuthor(User.Identity.Name);
            bool isAdmin = User.IsInRole("Admin");

            return isAdmin || isAuthor;
        }

        private void SetArticleTags(Article article, ArticleViewModel model, BlogDbContext db)
        {
            var tagsStrings = model.Tags
                .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLower())
                .Distinct();

            article.Tags.Clear();

            foreach (var tagString in tagsStrings)
            {
                Tag tag = db.Tags.FirstOrDefault(t => t.Name.Equals(tagString));

                if (tag == null)
                {
                    tag = new Tag() { Name = tagString };
                    db.Tags.Add(tag);
                }

                article.Tags.Add(tag);
            }
        }

    }
}