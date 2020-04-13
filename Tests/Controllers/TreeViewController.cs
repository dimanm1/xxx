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
                    .Select(x => new TreeViewNode(x))
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
                List<long> groupIds = new List<long>();
                if (row.Groups != null)
                {
                    groupIds = row.Groups
                        .Select(x => long.Parse(x.Id))
                        .ToList();
                }

                // Таблица Node содержит ParentName, чтобы не делать Self Join.
                if (row.ParentId != null)
                {
                    Node parent = context.Node
                        .AsEnumerable()
                        .First(x => x.Id == row.ParentId);

                    // Если проверять HasChildren, то минимум действий все рано равен 1, а максимум - 2.
                    parent.HasChildren = true;

                    context.Node.Attach(parent);
                    context.Entry(parent).State = EntityState.Modified;
                    context.SaveChanges();

                    row.ParentName = parent.Name;
                }

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
                    Tooltip = row.Tooltip
                };

                List<Group> newNodeGroups = context.Group
                    .AsEnumerable()
                    .Where(x => groupIds.Contains(x.Id))
                    .ToList();

                newNode.Group.AddRange(newNodeGroups);

                context.Node.Add(newNode);
                context.SaveChanges();
            }

            return Json(new[] { row }.ToDataSourceResult(request, ModelState));
        }



        [HttpPost]
        public ActionResult TreeView_Update([DataSourceRequest]DataSourceRequest request, TreeViewNode row)
        {
            using (TreeViewEntities context = new TreeViewEntities())
            {
                List<long> groupIds = new List<long>();
                if (row.Groups != null && row.Groups.Any())
                {
                    groupIds = row.Groups
                        .Select(x => long.Parse(x.Id))
                        .ToList();
                }

                // Таблица Node содержит ParentName, чтобы не делать Self Join.
                if (row.ParentId != null)
                {
                    Node parent = context.Node
                        .AsEnumerable()
                        .First(x => x.Id == row.ParentId);

                    // Если проверять HasChildren, то минимум действий все рано равен 1, а максимум - 2.
                    parent.HasChildren = true;

                    context.Node.Attach(parent);
                    context.Entry(parent).State = EntityState.Modified;
                    context.SaveChanges();

                    row.ParentName = parent.Name;
                }
                else
                {
                    row.ParentName = null;
                    row.ParentId = null;
                }

                Node newNode = context.Node.First(x => x.Id == row.Id);

                newNode.Action = row.Action;
                newNode.Area = row.Area;
                newNode.Controller = row.Controller;
                newNode.Description = row.Description;
                newNode.HasChildren = row.HasChildren;
                newNode.Name = row.Name;
                newNode.ParentId = row.ParentId;
                newNode.ParentName = row.ParentName;
                newNode.Tooltip = row.Tooltip;
                newNode.Group.Clear();

                if (groupIds.Any())
                {
                    newNode.Group.AddRange(context.Group
                        .Where(x => groupIds.Contains(x.Id)));
                }
                
                context.Node.Attach(newNode);
                context.Entry(newNode).State = EntityState.Modified;
                context.SaveChanges();

                Node[] children = context.Node
                    .Where(x => x.ParentId == newNode.Id)
                    .ToArray();

                foreach (Node child in children)
                {
                    child.ParentName = newNode.Name;
                    context.Node.Attach(child);
                    context.Entry(child).State = EntityState.Modified;   
                }
                context.SaveChanges();


            }

            return Json(new[] { row }.ToDataSourceResult(request, ModelState));
        }



        public JsonResult AlterParents(string selectedRow_Id)
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
                    }).ToList();

                if (string.IsNullOrEmpty(selectedRow_Id))
                {
                    return Json(alterParents, JsonRequestBehavior.AllowGet);
                }

                // Узел не может быть сам себе родителем.

                var t = alterParents
                        .Where(x => x.Id != selectedRow_Id)
                        .ToList();


                return Json(t,
                    JsonRequestBehavior.AllowGet);
            }
        }
        


        public JsonResult Groups(string selectedRow_ParentId)
        {
            using (TreeViewEntities context = new TreeViewEntities())
            {
                if (string.IsNullOrEmpty(selectedRow_ParentId))
                {
                    var allGroups = context.Group
                        .AsEnumerable()
                        .Select(x => new
                        {
                            Id = x.Id.ToString(),
                            Name = x.Name
                        })
                        .ToList();

                    return Json(allGroups, JsonRequestBehavior.AllowGet);
                }

                var nodeGroups = context.Node
                    .AsEnumerable()
                    .First(x => x.Id == long.Parse(selectedRow_ParentId)).Group
                    .Select(x => new
                    {
                        Id = x.Id.ToString(),
                        Name = x.Name
                    })
                    .ToList();

                return Json(nodeGroups, JsonRequestBehavior.AllowGet);
            }
        }



    
    }
}