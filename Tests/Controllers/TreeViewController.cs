using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tests.Models;

namespace Tests.Controllers
{
    public class TreeViewController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TreeView_Read([DataSourceRequest]DataSourceRequest request)
        {
            using (TreeViewEntities context = new TreeViewEntities())
            {
                IQueryable<TreeViewNode> table = context.Node
                    .AsEnumerable()
                    .Select(x => new TreeViewNode(x)
                    {
                        Groups = x.Group
                            .Select(g => new TreeViewNode_Group
                            {
                                Id = g.Id.ToString(),
                                Name = g.Name
                            })
                            .ToArray()
                    })
                    .AsQueryable();

                DataSourceResult result = table.ToDataSourceResult(request);
                return Json(result);
            }
        }

        [HttpPost]
        public ActionResult TreeView_Create([DataSourceRequest]DataSourceRequest request, TreeViewNode row)
        {
            using (TreeViewEntities context = new TreeViewEntities())
            {
                // Таблица Node содержит ParentName, чтобы не делать Self Join.
                if (row.ParentId != null)
                {
                    Node parent = context.Node
                        .AsEnumerable()
                        .First(x => x.Id == row.ParentId);

                    // Если проверять HasChildren, то минимум действий все рано равен 1, а максимум - 2.
                    parent.HasChildren = true;

                    //context.Node.Attach(parent);
                    //context.Entry(parent).State = EntityState.Modified;
                    //context.SaveChanges();

                    row.ParentName = parent.Name;
                }

                List<long> groupIds = row.Groups
                    .Select(x => long.Parse(x.Id))
                    .ToList();

                Node newNode = new Node
                {
                    Action = row.Action,
                    Area = row.Area,
                    Controller = row.Controller,
                    Description = row.Description,
                    HasChildren = row.HasChildren,
                    Name = row.Name,
                    ParentId = row.ParentId,
                    ParentName = row.ParentName,
                };

                List<Group> newNodeGroups = context.Group
                    .AsEnumerable()
                    .Where(x => groupIds.Contains(x.Id))
                    .ToList();

                newNode.Group.AddRange(newNodeGroups);

                context.Node.Add(newNode);
                context.SaveChanges();

                foreach (var group in newNodeGroups)
                {
                    context.Group.Attach(group);
                    context.Entry(group).State = EntityState.Modified;
                }

                context.SaveChanges();
            }

            return Json(new[] { row }.ToDataSourceResult(request, ModelState));
        }







        public JsonResult AlterParents(string clientId)
        {
            using (TreeViewEntities context = new TreeViewEntities())
            {
                var alterParents = context.Node
                    .AsEnumerable()
                    .Select(x => new
                    {
                        Id = x.Id.ToString(),
                        Name = x.Name,
                        ParentName = string.IsNullOrEmpty(x.ParentName) ? "IS ROOT" : x.ParentName,
                        Description = x.Description
                    });

                if (string.IsNullOrEmpty(clientId))
                {
                    return Json(alterParents.ToList(), JsonRequestBehavior.AllowGet);
                }

                return Json(alterParents.SkipWhile(x => x.Id == clientId).ToList(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Groups(string clientId)
        {
            using (TreeViewEntities context = new TreeViewEntities())
            {
                var groups = context.Group
                    .AsEnumerable()
                    .Select(x => new
                    {
                        Id = x.Id.ToString(),
                        Name = x.Name
                    })
                    .ToList();

                if (string.IsNullOrEmpty(clientId))
                {
                    return Json(groups, JsonRequestBehavior.AllowGet);
                }

                long? ParentId = context.Node
                    .AsEnumerable()
                    .First(x => x.Id == int.Parse(clientId))
                    .ParentId;

                if (ParentId == null)
                {
                    return Json(groups, JsonRequestBehavior.AllowGet);
                }

                groups = context.Node
                    .AsEnumerable()
                    .First(x => x.Id == ParentId).Group
                    .Select(x=>new
                    {
                        Id = x.Id.ToString(),
                        Name = x.Name
                    })
                    .ToList();

                return Json(groups, JsonRequestBehavior.AllowGet);
            }
        }
    }
}