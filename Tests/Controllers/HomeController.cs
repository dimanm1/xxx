using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tests.Models;

namespace Tests.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetMenu(int? id)
        {
            var menu = from e in new TreeViewEntities().Node
                       where (id.HasValue ? e.ParentId == id : e.ParentId == null)
                       select new
                       {
                           id = e.Id,
                           Name = e.Name,
                           hasChildren = e.HasChildren
                       };

            return Json(menu, JsonRequestBehavior.AllowGet);
        }
    }
}