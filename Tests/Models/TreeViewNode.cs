using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Tests.Models
{
    // На сервер значения передаются в одноименные свойства, а не поля.
    public class TreeViewNode_Group
    {
        public string Id { get; set; }
        public string Name { get; set; }
        
    }

    

    public class TreeViewNode: Node
    {
        // На сервер значения передаются в одноименные свойства, а не поля.
        public TreeViewNode_Group[] Groups { get; set; }

        public string GroupsString { get; set; }


        public TreeViewNode(Node node)
        {
            Action = node.Action;
            Area = node.Area;
            Controller = node.Controller;
            Description = node.Description;
            //Group = node.Group;
            Groups = node.Group
                .Select(g => new TreeViewNode_Group
                {
                    Id = g.Id.ToString(),
                    Name = g.Name
                })
                .ToArray();
            HasChildren = node.HasChildren;
            Id = node.Id;
            Name = node.Name;
            ParentId = node.ParentId;
            ParentName = node.ParentName;
            Tooltip = node.Tooltip;
            GroupsString = string.Join(",", node.Group
                .Select(g => g.Name).ToArray());
        }
        public TreeViewNode() { }
    }
}