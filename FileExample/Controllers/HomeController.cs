using FileExample.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FileExample.Controllers {
    public class HomeController : Controller {
        private TestDBEntities _db = new TestDBEntities();
        protected override void Dispose(bool disposing) {
            if (disposing) {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        //如果使用者輸入沒有的action，會導回首頁
        protected override void HandleUnknownAction(string actionName) {
            Response.Redirect("/");
            base.HandleUnknownAction(actionName);
        }

        // Index Action-------------------

        public ActionResult Index() {
            return View();
        }


        // GetData Action-------------------
        public ActionResult GetData(int? page, string txtsearch) {
            var model = _db.Media.AsQueryable();
            model = from n in _db.Media
                    where n.Category == 5
                    select n;
            if (string.IsNullOrWhiteSpace(txtsearch) == false) {
                model = model.Where(x => x.Title.Contains(txtsearch) || x.Yearx.Contains(txtsearch));
            }
            model = model.OrderByDescending(x => x.Yearx);
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(model.ToPagedList(pageNumber, pageSize));
        }

  





        // Create Action-------------------
        public ActionResult Create() {
            Media Act = new Media();
            return View(Act);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Media Act, HttpPostedFileBase[] files) {
            if (!ModelState.IsValid) {
                return View();
            }
            var results = from c in _db.Media
                          where c.Category == 5 && c.Yearx == Act.Yearx //如果Category ==5 並且 Yearx跟使用者填寫的年度一樣
                          select c; //results等於符合條件的資料
            if (results.Any()) { //有資料的話
                TempData["Error_Yearx"] = "此年度資料已新增，請重新填寫。";
                return View();
            } else {
                Act.Category = 5;
                Act.LinkType = 0;
                _db.Media.Add(Act);
                _db.SaveChanges();
                foreach (var file in files) {
                    if (file != null && file.ContentLength > 0) {
                        ActivityFile actFile = new ActivityFile();
                        actFile.Filename = file.FileName;
                        actFile.MID = Act.MID;
                        actFile.Created = DateTime.Now;
                        var fileName = Path.GetFileName(file.FileName);
                        var pathlocat = Server.MapPath("~/Files/ActivityPhotoFile/" + Act.MID + "/");
                        var path = Path.Combine(pathlocat, fileName);
                        if (Directory.Exists(pathlocat)) {
                            file.SaveAs(path);
                        } else {
                            Directory.CreateDirectory(pathlocat);
                            file.SaveAs(path);
                        }
                        _db.ActivityFile.Add(actFile);
                        _db.SaveChanges();
                    }
                }
                TempData["result"] = "新增成功。";
            }
            return RedirectToAction("Index");
        }

    }
}