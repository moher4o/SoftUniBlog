using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog.Models;

namespace Blog.Controllers
{
    public class ImageController : Controller
    {
        private BlogDbContext db = new BlogDbContext();

        // GET: Images
        public ActionResult Index()
        {
            var articleImages = db.ArticleImages.Include(a => a.Article);
            return View(articleImages.ToList());
        }

        //GET
        public ActionResult AddImage(int id, string title)
        {
            var model = new ArticleImage();
            model.ArticleId = id;
            ViewBag.articleTitle = title;

            return View(model);
            
        }
        [HttpPost]
        public ActionResult AddImage(ArticleImage model, HttpPostedFileBase file)
        {
            if (file != null)
             {
                using (var db = new BlogDbContext())
                {
                   //var lastImage = new ArticleImage();
                    var lastImage = db.ArticleImages.Count();
                    int nextId = db.ArticleImages.Max(i => i.Id)+ 1;
                    //if (lastImage.Id > 0)
                    //{
                    //    nextId = lastImage.Id + 1;
                    //}
                    
                    var image = new ArticleImage();
                    image.ArticleId = model.ArticleId;
                    string pic = System.IO.Path.GetFileName(file.FileName);
                    pic = nextId.ToString() + pic;
                    image.FileName = pic;
                    db.ArticleImages.Add(image);
                    db.SaveChanges();

                    string path = System.IO.Path.Combine(
                                           Server.MapPath("~/images"), pic);
                    // file is uploaded
                    file.SaveAs(path);
                    
                }
            }
            return RedirectToAction("Details", "Article", new { id = model.ArticleId });
        }


        // GET: Images/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArticleImage articleImage = db.ArticleImages.Find(id);
            if (articleImage == null)
            {
                return HttpNotFound();
            }
            return View(articleImage);
        }

        // GET: Images/Create
        public ActionResult Create()
        {
            ViewBag.ArticleId = new SelectList(db.Articles, "Id", "Title");
            return View();
        }

        // POST: Images/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ArticleId,FileName")] ArticleImage articleImage)
        {
            if (ModelState.IsValid)
            {
                db.ArticleImages.Add(articleImage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ArticleId = new SelectList(db.Articles, "Id", "Title", articleImage.ArticleId);
            return View(articleImage);
        }

        // GET: Images/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArticleImage articleImage = db.ArticleImages.Find(id);
            if (articleImage == null)
            {
                return HttpNotFound();
            }
            ViewBag.ArticleId = new SelectList(db.Articles, "Id", "Title", articleImage.ArticleId);
            return View(articleImage);
        }

        // POST: Images/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ArticleId,FileName")] ArticleImage articleImage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(articleImage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ArticleId = new SelectList(db.Articles, "Id", "Title", articleImage.ArticleId);
            return View(articleImage);
        }

        // GET: Images/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArticleImage articleImage = db.ArticleImages.Find(id);
            if (articleImage == null)
            {
                return HttpNotFound();
            }
            return View(articleImage);
        }

        // POST: Images/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ArticleImage articleImage = db.ArticleImages.Find(id);
            db.ArticleImages.Remove(articleImage);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
